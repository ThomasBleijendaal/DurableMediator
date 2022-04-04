namespace DurableMediator;

public record WorkflowStatus(string Name, string InstanceId, WorkflowRuntimeStatus Status);
