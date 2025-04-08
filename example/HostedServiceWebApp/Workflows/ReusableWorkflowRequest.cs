using DurableMediator.HostedService;
using WorkflowHandlers.Responses;

namespace HostedServiceWebApp.Workflows;

public record ReusableWorkflowRequest(Guid SomeId) : IWorkflowRequest<ReusableWorkflowResponse>
{
    public string WorkflowName => nameof(ReusableWorkflow);

    public string InstanceId => $"reusable-{SomeId}";
}
