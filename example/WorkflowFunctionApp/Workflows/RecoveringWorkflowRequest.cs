using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record RecoveringWorkflowRequest(Guid FlakyResourceId) : IWorkflowRequest
{
    public string WorkflowName => "Recovering";

    public string InstanceId => $"recovering-{FlakyResourceId}";
}
