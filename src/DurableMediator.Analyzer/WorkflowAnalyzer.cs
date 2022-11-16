using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace DurableMediator.Analyzer
{
    [Generator]
    public class WorkflowAnalyzer : ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new WorkflowReceiver());
            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is WorkflowReceiver workflowReciever)
            {
                var processor = new WorkflowProcessor(context);

                var symbols = new List<INamedTypeSymbol>();

                foreach (var candidate in workflowReciever.Candidates)
                {
                    var list = processor.AnalyzeSteps(candidate);

                    var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);

                    var workflowSymbol = model.GetDeclaredSymbol(candidate);

                    if (workflowSymbol != null)
                    {
                        symbols.Add(workflowSymbol);

                        var sourceText = GenerateProvider.GenerateProviderSourceText(workflowSymbol, list);
                        context.AddSource($"{workflowSymbol.Name}.cs", sourceText);
                    }
                }

                var diSourceText = GenerateProvider.GenerateProviderDISourceText(symbols);
                context.AddSource("DI.cs", diSourceText);
            }
        }
    }

    internal sealed class WorkflowReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax &&
                typeDeclarationSyntax.AttributeLists.Any(x => x.Attributes.Any(x => x.Name.ToString() == "AnalyzeFlow")))
            {
                Candidates.Add(typeDeclarationSyntax);
            }
        }
    }

    internal static class CompilationExtensions
    {
        public static INamedTypeSymbol GetUnboundGenericType(this Compilation compilation, string name)
            => compilation.GetTypeByMetadataName(name)?.ConstructUnboundGenericType() ?? throw new InvalidOperationException($"Cannot find {name}");
        public static INamedTypeSymbol GetType(this Compilation compilation, string name)
            => compilation.GetTypeByMetadataName(name) ?? throw new InvalidOperationException($"Cannot find {name}");
    }

    internal class WorkflowProcessor
    {
        // TODO: see if these can be extracted from interface definitions
        public const int DefaultRetries = 3;

        private readonly GeneratorExecutionContext _context;

        private readonly INamedTypeSymbol _int;
        private readonly INamedTypeSymbol _requestInterface;

        public WorkflowProcessor(GeneratorExecutionContext context)
        {
            _context = context;

            _int = context.Compilation.GetType("System.Int32");
            _requestInterface = context.Compilation.GetUnboundGenericType("MediatR.IRequest`1");
        }

        public List<StepDescription> AnalyzeSteps(TypeDeclarationSyntax workflowType)
        {
            var orchestrateMethod = workflowType.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.Identifier.Text == "OrchestrateAsync");

            if (orchestrateMethod?.Body == null)
            {
                return new List<StepDescription>();
            }

            var methodBody = orchestrateMethod.Body;

            return Analyze(methodBody);
        }

        public List<StepDescription> Analyze(BlockSyntax block)
        {
            var steps = new List<StepDescription>();

            foreach (var statement in block.Statements)
            {
                var step = statement switch
                {
                    ExpressionStatementSyntax expression => Analyze(expression),

                    _ => null
                };

                if (step != null)
                {
                    steps.Add(step);
                }
            }

            return steps;
        }

        public StepDescription? Analyze(ExpressionStatementSyntax expression)
        {
            if (expression.Expression is AwaitExpressionSyntax await)
            {
                return Analyze(await);
            }

            return null;
        }

        public StepDescription? Analyze(AwaitExpressionSyntax await)
        {
            if (await.Expression is InvocationExpressionSyntax invocation)
            {
                return Analyze(invocation);
            }

            return null;
        }

        public StepDescription? Analyze(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var isSend = memberAccess.Name.Identifier.Text == "SendAsync";
                var isSendWithRetry = memberAccess.Name.Identifier.Text == "SendWithRetryAsync";

                if (isSend || isSendWithRetry)
                {
                    var model = _context.Compilation.GetSemanticModel(invocation.SyntaxTree);
                    var arguments = invocation.ArgumentList.Arguments.Select(argument => model.GetSymbolInfo(argument.Expression)).ToImmutableArray();
                    var requestType = arguments
                        .FirstOrDefault(a =>
                            a.Symbol != null &&
                            a.Symbol.ContainingType.AllInterfaces.Any(i =>
                                i.IsGenericType &&
                                i.ConstructUnboundGenericType().Equals(_requestInterface, SymbolEqualityComparer.Default)))
                        .Symbol?.ContainingType;

                    if (requestType == null)
                    {
                        return null;
                    }

                    var retries = 0;

                    if (isSendWithRetry)
                    {
                        var retryArgument = invocation.ArgumentList.Arguments.Select(x => x.Expression).OfType<LiteralExpressionSyntax>().FirstOrDefault(x => x.Kind() == SyntaxKind.NumericLiteralExpression);

                        if (!int.TryParse(retryArgument?.Token.ValueText, out retries))
                        {
                            retries = DefaultRetries;
                        }
                    }

                    return new SendStepDescription(requestType.GetFqns(), retries);
                }
            }

            return null;
        }
    }

    internal static class SymbolExtensions
    {
        public static string GetFqns(this INamedTypeSymbol type)
            => type.ContainingNamespace.IsGlobalNamespace
                ? type.Name
                : $"{type.ContainingNamespace.Name}.{type.Name}";
    }

    internal static class GenerateProvider
    {
        public static SourceText GenerateProviderDISourceText(IEnumerable<INamedTypeSymbol> symbols)
        {
            using var textWriter = new StringWriter();
            using var writer = new IndentedTextWriter(textWriter);

            writer.WriteLine($"public static class MetadataProvider");

            using (writer.Braces())
            {
                writer.WriteLine($"public IServiceCollection AddGeneratedMetadataProviders(this IServiceCollection services)");
                using (writer.Braces())
                {
                    foreach (var symbol in symbols)
                    {
                        var workflowName = symbol.Name;

                        writer.WriteLine($"services.AddSingleton<IWorkflowMetadataProvider, {workflowName}MetadataProvider>();");
                    }

                    writer.WriteLine("return services;");
                }
            }

            return SourceText.From(textWriter.ToString(), Encoding.UTF8);
        }

        public static SourceText GenerateProviderSourceText(INamedTypeSymbol symbol, List<StepDescription> steps)
        {
            var workflowName = symbol.Name;
            var workflowFqns = symbol.GetFqns();

            using var textWriter = new StringWriter();
            using var writer = new IndentedTextWriter(textWriter);

            writer.WriteLine($"public class {workflowName}MetadataProvider : IWorkflowMetadataProvider");
            using (writer.Braces())
            {
                writer.WriteLine($"public WorkflowMetadata GetFlow()");
                using (writer.Braces())
                {
                    writer.WriteLine($"return new WorkflowMetadata");
                    using (writer.Braces())
                    {
                        writer.WriteLine($"Type = typeof({workflowFqns}),");
                        writer.WriteLine("Steps = new List<WorkflowSteps>");
                        using (writer.Braces())
                        {
                            GenerateSteps(writer, steps);
                        }
                    }
                }
            }

            return SourceText.From(textWriter.ToString(), Encoding.UTF8);
        }

        public static void GenerateSteps(IndentedTextWriter writer, IEnumerable<StepDescription> steps)
        {
            foreach (var step in steps)
            {
                switch (step)
                {
                    case SendStepDescription sendStep:
                        GenerateStep(writer, sendStep);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void GenerateStep(IndentedTextWriter writer, SendStepDescription step)
        {
            writer.WriteLine("new WorkflowSend");
            using (writer.Braces())
            {
                writer.WriteLine($"Request = typeof({step.RequestType}),");
                writer.WriteLine($"Retries = {step.Retries}");
            }
        }
    }
}
