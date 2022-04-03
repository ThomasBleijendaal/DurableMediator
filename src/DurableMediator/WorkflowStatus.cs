namespace DurableMediator;

public record WorkflowStatus(string Name, string InstanceId, Guid? AffectedEntityId, WorkflowRuntimeStatus Status);
