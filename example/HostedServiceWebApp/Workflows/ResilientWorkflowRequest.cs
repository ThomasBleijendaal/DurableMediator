using DurableMediator.HostedService;
using MediatR;

namespace HostedServiceWebApp.Workflows;

public record ResilientWorkflowRequest(Guid DodgyResourceId) : IWorkflowRequest<Unit>
{
    public string WorkflowName => nameof(ResilientWorkflow);

    public string InstanceId => $"resilient-{DodgyResourceId}";
}
