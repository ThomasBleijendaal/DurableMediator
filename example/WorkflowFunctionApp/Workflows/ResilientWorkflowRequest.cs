using DurableMediator;

namespace WorkflowFunctionApp.Workflows;

internal record ResilientWorkflowRequest(Guid DodgyResourceId) : IWorkflowRequest
{
    public string WorkflowName => "Resilient";

    public string InstanceId => $"resilient-{DodgyResourceId}";
}
