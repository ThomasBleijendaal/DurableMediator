namespace DurableMediator;

internal record WorkflowRequestWrapper<TRequest>(
    Tracing Tracing,
    TRequest Request);
