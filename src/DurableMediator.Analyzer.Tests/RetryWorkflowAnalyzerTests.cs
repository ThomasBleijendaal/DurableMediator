using DurableMediator.Analyzer.Tests.Helpers;

namespace DurableMediator.Analyzer.Tests;

internal class RetryWorkflowAnalyzerTests
{
    [Test]
    public void TestRetryWorkflow()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using DurableMediator;
            using DurableMediator.Analyzer;
            using MediatR;

            public record SimpleRequest(Guid Id, string Description) : IRequest<BasicResponse>;
            public record BasicResponse(Guid Id) : IRequest<BasicResponse>;
            public record BasicWorkflowRequest(Guid Id) : IWorkflowRequest
            {
                public string WorkflowName => "Basic";
                public string InstanceId => Id.ToString();
            };

            [AnalyzeFlow]
            public record RetryWorkflow() : IWorkflow<BasicWorkflowRequest, Unit>
            {
                public async Task<Unit> OrchestrateAsync(IWorkflowExecution<BasicWorkflowRequest> execution)
                {
                    // workflows support sequential requests
                    await execution.SendWithRetryAsync(new SimpleRequest(execution.Request.Id, "1"), CancellationToken.None, 1);

                    return Unit.Value;
                }
            }
            """,
            """
            public class RetryWorkflowMetadataProvider : IWorkflowMetadataProvider
            {
                public WorkflowMetadata GetFlow()
                {
                    return new WorkflowMetadata
                    {
                        Type = typeof(RetryWorkflow),
                        Steps = new List<WorkflowSteps>
                        {
                            new WorkflowSend
                            {
                                Request = typeof(SimpleRequest),
                                Retries = 1
                            }
                        }
                    }
                }
            }

            """,
            """
            public static class MetadataProvider
            {
                public IServiceCollection AddGeneratedMetadataProviders(this IServiceCollection services)
                {
                    services.AddSingleton<IWorkflowMetadataProvider, RetryWorkflowMetadataProvider>();
                    return services;
                }
            }

            """);
    }

    [Test]
    public void TestImplicitRetryWorkflow()
    {
        GeneratorTestHelper.TestGeneratedCode("""
            using DurableMediator;
            using DurableMediator.Analyzer;
            using MediatR;

            public record SimpleRequest(Guid Id, string Description) : IRequest<BasicResponse>;
            public record BasicResponse(Guid Id) : IRequest<BasicResponse>;
            public record BasicWorkflowRequest(Guid Id) : IWorkflowRequest
            {
                public string WorkflowName => "Basic";
                public string InstanceId => Id.ToString();
            };

            [AnalyzeFlow]
            public record RetryWorkflow() : IWorkflow<BasicWorkflowRequest, Unit>
            {
                public async Task<Unit> OrchestrateAsync(IWorkflowExecution<BasicWorkflowRequest> execution)
                {
                    // workflows support sequential requests
                    await execution.SendWithRetryAsync(new SimpleRequest(execution.Request.Id, "1"), CancellationToken.None);

                    return Unit.Value;
                }
            }
            """,
            """
            public class RetryWorkflowMetadataProvider : IWorkflowMetadataProvider
            {
                public WorkflowMetadata GetFlow()
                {
                    return new WorkflowMetadata
                    {
                        Type = typeof(RetryWorkflow),
                        Steps = new List<WorkflowSteps>
                        {
                            new WorkflowSend
                            {
                                Request = typeof(SimpleRequest),
                                Retries = 3
                            }
                        }
                    }
                }
            }

            """,
            """
            public static class MetadataProvider
            {
                public IServiceCollection AddGeneratedMetadataProviders(this IServiceCollection services)
                {
                    services.AddSingleton<IWorkflowMetadataProvider, RetryWorkflowMetadataProvider>();
                    return services;
                }
            }

            """);
    }
}
