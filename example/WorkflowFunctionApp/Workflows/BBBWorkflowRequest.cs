using DurableMediator;

namespace WorkflowFunctionApp.Workflows
{
    internal record BBBWorkflowRequest(Guid Id) : IWorkflowRequest
    {
        public string InstanceId => $"bbb-{Id}";
    }
}
