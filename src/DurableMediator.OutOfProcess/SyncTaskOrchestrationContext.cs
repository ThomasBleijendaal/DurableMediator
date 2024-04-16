using System.Reflection;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

public sealed class SyncTaskOrchestrationContext : TaskOrchestrationContext
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<IDurableMediatorMiddleware> _middlewares;
    private readonly IWorkflowRequest _request;
    private readonly ISyncWorkflowClient _syncWorkflowClient;
    private readonly Guid _instanceIdFallback = Guid.NewGuid();

    public SyncTaskOrchestrationContext(
        IMediator mediator,
        ILoggerFactory loggerFactory,
        IEnumerable<IDurableMediatorMiddleware> middlewares,
        IWorkflowRequest request,
        ISyncWorkflowClient syncWorkflowClient)
    {
        _mediator = mediator;
        LoggerFactory = loggerFactory;
        _middlewares = middlewares;
        _request = request;
        _syncWorkflowClient = syncWorkflowClient;
    }

    public override TaskName Name => _request.WorkflowName;

    public override string InstanceId => _request.InstanceId ?? _instanceIdFallback.ToString();

    public override ParentOrchestrationInstance? Parent => null;

    public override DateTime CurrentUtcDateTime => DateTime.UtcNow;

    public override bool IsReplaying => false;

    protected override ILoggerFactory LoggerFactory { get; }

    public override async Task<TResult> CallActivityAsync<TResult>(TaskName name, object? input = null, TaskOptions? options = null)
    {
        var function = new DurableMediatorFunction(_mediator, _middlewares);

        if (input is MediatorRequest request)
        {
            await function.DurableMediatorAsync(request, InstanceId);
            return default!;
        }
        else if (input is MediatorRequestWithResponse requestWithResponse && typeof(TResult) == typeof(MediatorResponse))
        {
            var response = await function.DurableMediatorWithResponseAsync(requestWithResponse, InstanceId);
            return (TResult)(object)response;
        }
        else if (input is MediatorRequestWithCheckAndResponse requestWithCheck && typeof(TResult) == typeof(MediatorResponse))
        {
            var response = await function.DurableMediatorWithCheckAndResponseAsync(requestWithCheck, InstanceId);
            return (TResult)(object)response;
        }
        else
        {
            throw new InvalidOperationException("Invalid request for this context");
        }
    }

    public override Task<TResult> CallSubOrchestratorAsync<TResult>(TaskName orchestratorName, object? input = null, TaskOptions? options = null)
    {
        if (input is IWorkflowRequest)
        {
            var requestType = input.GetType();

            MethodInfo? method;

            if (requestType.GetInterface(typeof(IWorkflowRequest<>).Name) is Type requestWithResponseType)
            {
                var responseType = requestWithResponseType.GenericTypeArguments[0];
                var genericRequestType = typeof(IWorkflowRequest<>).MakeGenericType(Type.MakeGenericMethodParameter(0));

                method = typeof(ISyncWorkflowClient)
                    .GetMethod(nameof(ISyncWorkflowClient.RunWorkflowAsync), 1, [genericRequestType])
                    ?.MakeGenericMethod(responseType);
            }
            else
            {
                method = typeof(ISyncWorkflowClient)
                    .GetMethod(nameof(ISyncWorkflowClient.RunWorkflowAsync), 0, [typeof(IWorkflowRequest)]);
            }

            if (method == null)
            {
                throw new InvalidOperationException($"Cannot determine type of workflow");
            }

            return (method.Invoke(_syncWorkflowClient, [input]) as Task<TResult>)!;
        }
        else
        {
            throw new InvalidOperationException("Cannot start this type of sub orchestration from this context");
        }
    }

    public override void ContinueAsNew(object? newInput = null, bool preserveUnprocessedEvents = true)
        => throw new NotSupportedException("Cannot continue as new from this context");

    public override Task CreateTimer(DateTime fireAt, CancellationToken cancellationToken)
        => Task.Delay(fireAt - CurrentUtcDateTime, cancellationToken);

    public override T? GetInput<T>() where T : default
        => _request is T t ? t : default;

    public override Guid NewGuid()
        => Guid.NewGuid();

    public override void SendEvent(string instanceId, string eventName, object payload)
        => throw new NotSupportedException("Cannot send event from this context");

    public override void SetCustomStatus(object? customStatus)
        => throw new NotSupportedException("Cannot set custom status from this context");

    public override Task<T> WaitForExternalEvent<T>(string eventName, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Cannot wait for events from this context");
}
