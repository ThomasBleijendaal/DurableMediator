using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record ABCWorkflowRequest(Guid AbcId) : IWorkflowRequest
{
    public string WorkflowName => "ABC";

    public string InstanceId => $"abc-{AbcId}";
}
