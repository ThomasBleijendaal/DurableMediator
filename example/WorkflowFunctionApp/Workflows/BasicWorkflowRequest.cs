using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record BasicWorkflowRequest(Guid RequestId) : IWorkflowRequest
{
    public string WorkflowName => "Basic";

    public string InstanceId => $"basic-{RequestId}";
}
