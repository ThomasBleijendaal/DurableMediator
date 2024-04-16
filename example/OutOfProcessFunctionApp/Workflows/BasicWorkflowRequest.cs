using DurableMediator.OutOfProcess;

namespace OutOfProcessFunctionApp.Workflows;

public record BasicWorkflowRequest(Guid RequestId) : IWorkflowRequest
{
    public string WorkflowName => nameof(BasicWorkflow);

    public string InstanceId => $"basic-{RequestId}";
}
