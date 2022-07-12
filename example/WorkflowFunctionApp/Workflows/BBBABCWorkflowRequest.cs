using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record BBBABCWorkflowRequest(Guid BbbAbcId) : IWorkflowRequest
{
    public string WorkflowName => "BBBABC";

    public string InstanceId => $"bbbabc-{BbbAbcId}";
}
