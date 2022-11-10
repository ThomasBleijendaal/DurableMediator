using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableMediator.Analyzer.Tests
{
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
}
