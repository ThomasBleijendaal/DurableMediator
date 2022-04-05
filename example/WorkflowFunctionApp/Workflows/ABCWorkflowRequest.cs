using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record ABCWorkflowRequest(Guid Id) : IWorkflowRequest
{
    public string InstanceId => $"aaa-{Id}";
}
