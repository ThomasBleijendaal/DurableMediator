using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record ABCWorkflowRequest(Guid AbcId) : IWorkflowRequest
{
    public string InstanceId => $"abc-{AbcId}";
}
