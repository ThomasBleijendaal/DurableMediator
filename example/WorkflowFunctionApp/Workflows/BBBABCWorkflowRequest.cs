using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record BBBABCWorkflowRequest(Guid BbbAbcId) : IWorkflowRequest
{
    public string InstanceId => $"bbbabc-{BbbAbcId}";
}
