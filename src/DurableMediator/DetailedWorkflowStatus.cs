using Newtonsoft.Json.Linq;

namespace DurableMediator;

public record DetailedWorkflowStatus<TRequest>(
    string Name,
    string InstanceId,
    WorkflowRuntimeStatus Status,
    TRequest Request,
    DateTime CreateTime,
    DateTime LastUpdateTime,
    string? ExceptionMessage,
    JArray? History) : WorkflowStatus<TRequest>(
        Name,
        InstanceId,
        Status,
        Request,
        CreateTime,
        LastUpdateTime,
        ExceptionMessage);

public record DetailedWorkflowStatus<TRequest, TResponse>(
    string Name,
    string InstanceId,
    WorkflowRuntimeStatus Status,
    TRequest Request,
    TResponse? Response,
    DateTime CreateTime,
    DateTime LastUpdateTime,
    string? ExceptionMessage,
    JArray? History) : WorkflowStatus<TRequest, TResponse>(
        Name,
        InstanceId,
        Status,
        Request,
        Response,
        CreateTime,
        LastUpdateTime,
        ExceptionMessage);
