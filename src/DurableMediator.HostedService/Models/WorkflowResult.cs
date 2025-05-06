using DurableTask.Core;

namespace DurableMediator.HostedService.Models;

public record WorkflowResult
{
    public required OrchestrationState State { get; init; }
}

public record WorkflowResult<TWorkflowResponse> : WorkflowResult
{
    public TWorkflowResponse? Result { get; init; }
}
