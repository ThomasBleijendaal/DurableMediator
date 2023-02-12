using DurableMediator.OutOfProcess;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Workflows;

public record ReusableWorkflowRequest(Guid SomeId) : IWorkflowRequest<ReusableWorkflowResponse>
{
    public string WorkflowName => nameof(ReusableWorkflow);

    public string InstanceId => $"reusable-{SomeId}";
}
