using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record BBBWorkflowRequest(Guid Id) : IWorkflowRequest<BBBWorkflowResponse>
{
    public string InstanceId => $"bbb-{Id}";
}
