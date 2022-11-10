using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DurableMediator.Analyzer
{
    [Generator]
    public class WorkflowAnalyzer : ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new WorkflowReceiver());
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is WorkflowReceiver workflowReciever)
            {
                var processor = new WorkflowProcessor(context);

                foreach (var candidate in workflowReciever.Candidates)
                {
                    var list = processor.AnalyzeSteps(candidate);

                    var workflowName = candidate.Identifier.Text;
                    var sourceText = GenerateProvider.Generate(workflowName, list);

                    context.AddSource(workflowName, sourceText);
                }
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

    internal class WorkflowProcessor
    {
        private readonly GeneratorExecutionContext _context;

        public WorkflowProcessor(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public List<WorkflowStep> AnalyzeSteps(TypeDeclarationSyntax workflowType)
        {
            var orchestrateMethod = workflowType.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.Identifier.Text == "OrchestrateAsync");

            if (orchestrateMethod?.Body == null)
            {
                return new List<WorkflowStep>();
            }

            var methodBody = orchestrateMethod.Body;

            return Analyze(methodBody);
        }
    
        public List<WorkflowStep> Analyze(BlockSyntax block)
        {
            var steps = new List<WorkflowStep>();

            foreach (var statement  in block.Statements)
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

        public WorkflowStep? Analyze(ExpressionStatementSyntax expression)
        {
            if (expression.Expression is AwaitExpressionSyntax await)
            {
                return Analyze(await);
            }

            return null;
        }

        public WorkflowStep? Analyze(AwaitExpressionSyntax await)
        {
            if (await.Expression is InvocationExpressionSyntax invocation)
            {
                return Analyze(invocation);
            }

            return null;
        }

        public WorkflowStep? Analyze(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "SendAsync")
            {
                var model = _context.Compilation.GetSemanticModel(invocation.SyntaxTree);

                var x = model.GetTypeInfo(invocation.ArgumentList.Arguments.First());

                var methodSymbol = model.GetSymbolInfo(invocation.ArgumentList.Arguments.First().Expression);


                var type = methodSymbol.Symbol.ContainingType;

                //var requestInfo = methodSymbol.Parameters.First();

                var responseInfo = model.GetTypeInfo(invocation);


            }

            return null;
        }
    }

    internal static class GenerateProvider
    {
        public static SourceText Generate(string workflowName, List<WorkflowStep> steps)
        {
            using var textWriter = new StringWriter();
            using var writer = new IndentedTextWriter(textWriter);

            writer.WriteLine($"public class {workflowName}MetadataProvider : IWorkflowMetadataProvider<{workflowName}>");
            using (writer.Braces())
            {
                writer.WriteLine($"public WorkflowFlow<{workflowName}> GetFlow()");
                using (writer.Braces())
                {
                    writer.WriteLine($"return new WorkflowFlow<{workflowName}>");
                    using (writer.Braces())
                    {
                        writer.WriteLine("Steps = new List<WorkflowSteps>");
                        using (writer.Braces())
                        {
                            
                        }
                    }
                }
            }

            return SourceText.From(textWriter.ToString(), Encoding.UTF8);
        }
    }
}
