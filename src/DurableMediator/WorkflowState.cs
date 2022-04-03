namespace DurableMediator;

internal record WorkflowState(string WorkflowName, Guid? AffectedEntityId, string? ExceptionMessage);
