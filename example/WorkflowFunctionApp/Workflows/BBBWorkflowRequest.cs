using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record BBBWorkflowRequest(Guid BbbId) : IWorkflowRequest<BBBWorkflowResponse>
{
    public string InstanceId => $"bbb-{BbbId}";
}
