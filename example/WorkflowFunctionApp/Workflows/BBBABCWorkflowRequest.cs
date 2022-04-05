using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record BBBABCWorkflowRequest(Guid Id) : IWorkflowRequest
{
    public string InstanceId => $"abcbbb-{Id}";
}
