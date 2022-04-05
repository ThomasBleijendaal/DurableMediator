using DurableMediator;

namespace WorkflowFunctionApp.Workflows
{
    internal record ABCBBBWorkflowRequest(Guid Id) : IWorkflowRequest
    {
        public string InstanceId => $"abcbbb-{Id}";
    }
}
