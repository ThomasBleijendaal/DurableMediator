using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DurableMediator.Analyzer.Tests.Helpers;

namespace DurableMediator.Analyzer.Tests;

internal class BasicWorkflowAnalyzerTests
{
    [Test]
    public void TestBasicWorkflow()
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
            public record BasicWorkflow() : IWorkflow<BasicWorkflowRequest, Unit>
            {
                public async Task<Unit> OrchestrateAsync(IWorkflowExecution<BasicWorkflowRequest> execution)
                {
                    // workflows support sequential requests
                    await execution.SendAsync(new SimpleRequest(execution.Request.Id, "1"));

                    return Unit.Value;
                }
            }
            
            """,
            """
            public class BasicWorkflowMetadataProvider : IWorkflowMetadataProvider<BasicWorkflow>
            {
                public WorkflowFlow<BasicWorkflow> GetFlow()
                {
                    return new WorkflowFlow<BasicWorkflow>
                    {
                        Steps = new List<WorkflowSteps>
                        {
                            new WorkflowSend
                            {
                                Request = typeof(SendRequest),
                                Response = typeof(BasicResponse),
                                Retries = 0
                            }
                        }
                    }
                }
            }
            """);
    }
}
