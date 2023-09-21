using DurableMediator.OutOfProcess;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Workflows;

public record ReusableWorkflowRequest(Guid SomeId) : IWorkflowRequest<ReusableWorkflowResponse>
{
    public static Type Workflow => typeof(ReusableWorkflow);

    public string InstanceId => $"reusable-{SomeId}";
}
