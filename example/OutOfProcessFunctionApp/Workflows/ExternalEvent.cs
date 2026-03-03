using DurableMediator.OutOfProcess;

namespace OutOfProcessFunctionApp.Workflows;

public record ExternalEvent(string EventNr, Guid WorkflowId, Guid EventId) : IWorkflowEvent<ExternalEventWorkflowRequest>
{
    public string EventName => $"external-event-{EventNr}";

    public string InstanceId => $"external-event-{WorkflowId}";

    public ExternalEventWorkflowRequest? GetWorkflowRequest() => new ExternalEventWorkflowRequest(WorkflowId);
}
