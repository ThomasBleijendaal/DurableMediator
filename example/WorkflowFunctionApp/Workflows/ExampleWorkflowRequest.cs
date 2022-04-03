using DurableMediator;

namespace WorkflowFunctionApp.Workflows
{
    internal record ExampleWorkflowRequest(Guid Id) : IWorkflowRequest
    {
        public string InstanceId => $"example-{Id}";

        public Guid? AffectedEntityId => Id;
    }
}
