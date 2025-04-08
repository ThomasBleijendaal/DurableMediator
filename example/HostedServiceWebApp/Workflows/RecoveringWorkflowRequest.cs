using DurableMediator.HostedService;
using MediatR;

namespace HostedServiceWebApp.Workflows;

public record RecoveringWorkflowRequest(Guid FlakyResourceId) : IWorkflowRequest<Unit>
{
    public string WorkflowName => nameof(RecoveringWorkflow);

    public string InstanceId => $"recovering-{FlakyResourceId}";
}
