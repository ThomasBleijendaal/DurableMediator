using DurableMediator.HostedService;

namespace HostedServiceWebApp.Workflows;

public record ExternalEventWorkflowRequest(Guid RequestId) : IWorkflowRequest
{
    public string WorkflowName => nameof(ExternalEventWorkflow);

    public string? InstanceId => $"external-event-{RequestId}";
}
