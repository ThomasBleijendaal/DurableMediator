using System.Text.Json;
using DurableMediator.HostedService.Constants;
using DurableMediator.HostedService.Extensions;
using DurableMediator.HostedService.Managers;
using DurableMediator.HostedService.Models;
using DurableTask.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService;

internal class OrchestratorExecutor<TWorkflowRequest> : IWorkflowExecution<TWorkflowRequest>
{
    private readonly OrchestrationContext _context;
    private readonly TWorkflowRequest _request;
    private readonly ILogger _logger;

    private int _newGuidCounter;

    public OrchestratorExecutor(OrchestrationContext context, TWorkflowRequest request, ILogger logger)
    {
        _context = context;
        _request = request;
        _logger = logger;
    }

    public OrchestrationContext OrchestrationContext => _context;

    public ILogger ReplaySafeLogger => _logger;

    public Guid NewGuid()
    {
        var guidNameValue = $"{_context.OrchestrationInstance.InstanceId}_{_context.CurrentUtcDateTime:o}_{_newGuidCounter}";

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
        await _context.CreateTimer(OrchestrationContext.CurrentUtcDateTime.Add(delay ?? TimeSpan.FromSeconds(1)), token);

        await ExecuteRequestAsync(request, null);
    }
    public async Task<TResponse> SendWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        TimeSpan? delay,
        CancellationToken token)
    {
        await _context.CreateTimer(OrchestrationContext.CurrentUtcDateTime.Add(delay ?? TimeSpan.FromSeconds(1)), token);

        return await ExecuteRequestAsync(request, null);
    }

    public async Task<TResponse> SendWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        int maxAttempts = 3,
        TimeSpan? delay = null)
    {
        var response = await _context.CallDurableMediatorWithCheckAndResponseAsync(
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
        return _context.CreateSubOrchestrationInstance<dynamic?>(request.WorkflowName, Versions.Default, request.InstanceId, request);
    }

    public Task<TSubWorkflowResponse?> CallSubWorkflowAsync<TSubWorkflowResponse>(IWorkflowRequest<TSubWorkflowResponse> request)
    {
        return _context.CreateSubOrchestrationInstance<TSubWorkflowResponse?>(request.WorkflowName, Versions.Default, request.InstanceId, request);
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken token)
    {
        return _context.CreateTimer(DateTime.UtcNow.Add(delay), token);
    }

    private async Task ExecuteRequestAsync(IRequest request, RetryOptions? taskOptions)
    {
        await _context.CallDurableMediatorAsync(new MediatorRequest(request), taskOptions);
    }

    private async Task<TResponse> ExecuteRequestAsync<TResponse>(IRequest<TResponse> request, RetryOptions? taskOptions)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await _context.CallDurableMediatorAsync(new MediatorRequest(request), taskOptions);

            return default!;
        }

        var response = await _context.CallDurableMediatorWithResponseAsync(new MediatorRequestWithResponse(request), taskOptions)
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
