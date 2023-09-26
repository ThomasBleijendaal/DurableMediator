using DurableMediator.OutOfProcess;
using MediatR;

namespace OutOfProcessFunctionApp.Workflows;

public record ResilientWorkflowRequest(Guid DodgyResourceId) : IWorkflowRequest<Unit>
{
    public string WorkflowName => nameof(ResilientWorkflow);

    public string InstanceId => $"resilient-{DodgyResourceId}";
}
