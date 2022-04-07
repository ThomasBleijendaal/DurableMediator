namespace DurableMediator;

public record WorkflowStatus(
    string Name,
    string InstanceId, 
    WorkflowRuntimeStatus Status, 
    DateTime CreateTime,
    DateTime LastUpdateTime,
    string? ExceptionMessage);
