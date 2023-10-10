using DurableMediator.OutOfProcess;
using MediatR;

namespace OutOfProcessFunctionApp.Workflows;

public record RecoveringWorkflowRequest(Guid FlakyResourceId) : IWorkflowRequest<Unit>
{
    public string WorkflowName => nameof(RecoveringWorkflow);

    public string InstanceId => $"recovering-{FlakyResourceId}";
}
