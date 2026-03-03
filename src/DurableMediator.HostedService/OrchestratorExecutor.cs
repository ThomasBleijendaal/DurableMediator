using System.Text.Json;
using DurableMediator.HostedService.Constants;
using DurableMediator.HostedService.Extensions;
using DurableMediator.HostedService.Managers;
using DurableMediator.HostedService.Models;
using DurableMediator.HostedService.Plumbing;
using DurableTask.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService;

internal partial class OrchestratorExecutor<TWorkflowRequest> : IWorkflowExecution<TWorkflowRequest>
{
    private readonly WorkflowOrchestrationContext _ctx;
    private readonly TWorkflowRequest _request;
    private readonly ILogger _logger;

    private int _newGuidCounter;

    public OrchestratorExecutor(WorkflowOrchestrationContext ctx, TWorkflowRequest request, ILogger logger)
    {
        _ctx = ctx;
        _request = request;
        _logger = logger;
    }

    public OrchestrationContext OrchestrationContext => _ctx.OrchestrationContext;

    public ILogger ReplaySafeLogger => _logger;

    public Guid NewGuid()
    {
        var guidNameValue = $"{_ctx.OrchestrationContext.OrchestrationInstance.InstanceId}_{_ctx.OrchestrationContext.CurrentUtcDateTime:o}_{_newGuidCounter}";

        _newGuidCounter++;

        return GuidManager.CreateDeterministicGuid(GuidManager.DefaultNamespace, guidNameValue);
    }

    public TWorkflowRequest Request => _request;

    public Task SendAsync(IRequest request)
        => ExecuteRequestAsync(request, null);

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ExecuteRequestAsync(request, null);

    public Task SendWithRetryAsync(
        IRequest request,
        int maxAttempts = 3,
        TimeSpan? delay = null)
        => ExecuteRequestAsync(request, DefaultRetryOptions(maxAttempts, delay));

    public Task<TResponse> SendWithRetryAsync<TResponse>(
        IRequest<TResponse> request,
        int maxAttempts = 3,
        TimeSpan? delay = null)
        => ExecuteRequestAsync(request, DefaultRetryOptions(maxAttempts, delay));

    public async Task SendWithDelayAsync(
        IRequest request,
        TimeSpan? delay,
        CancellationToken token)
    {
        await _ctx.OrchestrationContext.CreateTimer<object?>(OrchestrationContext.CurrentUtcDateTime.Add(delay ?? TimeSpan.FromSeconds(1)), null, token);

        await ExecuteRequestAsync(request, null);
    }
    public async Task<TResponse> SendWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        TimeSpan? delay,
        CancellationToken token)
    {
        await _ctx.OrchestrationContext.CreateTimer<object?>(OrchestrationContext.CurrentUtcDateTime.Add(delay ?? TimeSpan.FromSeconds(1)), null, token);

        return await ExecuteRequestAsync(request, null);
    }

    public async Task<TResponse> SendWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        int maxAttempts = 3,
        TimeSpan? delay = null)
    {
        var response = await _ctx.OrchestrationContext.CallDurableMediatorWithCheckAndResponseAsync(
            new MediatorRequestWithCheckAndResponse(request, checkIfRequestApplied),
            DefaultRetryOptions(maxAttempts, delay))
            ?? throw new InvalidOperationException("Received an empty response");

        if (response.Response == null)
        {
            return default!;
        }

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }

    public Task CallSubWorkflowAsync(IWorkflowRequest request)
    {
        return _ctx.OrchestrationContext.CreateSubOrchestrationInstance<dynamic?>(request.WorkflowName, Versions.Default, request.InstanceId, request);
    }

    public async Task<TSubWorkflowResponse?> CallSubWorkflowAsync<TSubWorkflowResponse>(IWorkflowRequest<TSubWorkflowResponse> request)
    {
        var result = await _ctx.OrchestrationContext.CreateSubOrchestrationInstance<TSubWorkflowResponse?>(request.WorkflowName, Versions.Default, request.InstanceId, request);
        return result;
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken token)
    {
        return _ctx.OrchestrationContext.CreateTimer<object?>(DateTime.UtcNow.Add(delay), null, token);
    }

    public Task<T?> WaitForEventAsync<T>(string eventName, TimeSpan timeout)
        where T : IWorkflowEvent
    {
        return _ctx.GetEventTaskAsync<T>(eventName, timeout);
    }

    private async Task ExecuteRequestAsync(IRequest request, RetryOptions? taskOptions)
    {
        await _ctx.OrchestrationContext.CallDurableMediatorAsync(new MediatorRequest(request), taskOptions);
    }

    private async Task<TResponse> ExecuteRequestAsync<TResponse>(IRequest<TResponse> request, RetryOptions? taskOptions)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await _ctx.OrchestrationContext.CallDurableMediatorAsync(new MediatorRequest(request), taskOptions);

            return default!;
        }

        var response = await _ctx.OrchestrationContext.CallDurableMediatorWithResponseAsync(new MediatorRequestWithResponse(request), taskOptions)
            ?? throw new InvalidOperationException("Received an empty response");

        if (response.Response == null)
        {
            return default!;
        }
        else if (response.Response is TResponse typedResponse)
        {
            return typedResponse;
        }

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }

    private static RetryOptions DefaultRetryOptions(int maxAttempts, TimeSpan? delay)
        => new RetryOptions(DelayOrDefault(delay), maxAttempts)
        {
            BackoffCoefficient = 2
        };

    private static TimeSpan DelayOrDefault(TimeSpan? delay)
        => delay ?? TimeSpan.FromMilliseconds(Random.Shared.Next(500, 800));
}
