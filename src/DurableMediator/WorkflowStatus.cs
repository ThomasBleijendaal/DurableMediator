using Newtonsoft.Json.Linq;

namespace DurableMediator;

public record WorkflowStatus<TRequest>(
    string Name,
    string InstanceId,
    WorkflowRuntimeStatus Status,
    TRequest Request,
    DateTime CreateTime,
    DateTime LastUpdateTime,
    string? ExceptionMessage);

public record WorkflowStatus<TRequest, TResponse>(
    string Name,
    string InstanceId, 
    WorkflowRuntimeStatus Status, 
    TRequest Request,
    TResponse? Response,
    DateTime CreateTime,
    DateTime LastUpdateTime,
    string? ExceptionMessage);
