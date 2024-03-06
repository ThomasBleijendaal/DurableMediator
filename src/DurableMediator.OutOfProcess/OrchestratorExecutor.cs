using System.Text.Json;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

internal class OrchestratorExecutor<TWorkflowRequest> : IWorkflowExecution<TWorkflowRequest>
{
    private readonly TaskOrchestrationContext _context;
    private readonly TWorkflowRequest _request;
    private readonly ILogger _logger;

    public OrchestratorExecutor(TaskOrchestrationContext context, TWorkflowRequest request, ILogger logger)
    {
        _context = context;
        _request = request;
        _logger = logger;
    }

    public TaskOrchestrationContext OrchestrationContext => _context;

    public ILogger ReplaySafeLogger => _logger;

    public TWorkflowRequest Request => _request;

    public Task SendAsync(IRequest request)
        => ExecuteRequestAsync(request, null);

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ExecuteRequestAsync(request, null);

    public Task SendWithRetryAsync(
        IRequest request,
        int maxAttempts = 3,
        TimeSpan? delay = null)
        => ExecuteRequestAsync(request, DefaultTaskOptions(maxAttempts, delay));

    public Task<TResponse> SendWithRetryAsync<TResponse>(
        IRequest<TResponse> request,
        int maxAttempts = 3,
        TimeSpan? delay = null)
        => ExecuteRequestAsync(request, DefaultTaskOptions(maxAttempts, delay));

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
            DefaultTaskOptions(maxAttempts, delay))
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
        return _context.CallSubOrchestratorAsync(request.WorkflowName, request);
    }

    public Task<TSubWorkflowResponse?> CallSubWorkflowAsync<TSubWorkflowResponse>(IWorkflowRequest<TSubWorkflowResponse> request)
    {
        return _context.CallSubOrchestratorAsync<TSubWorkflowResponse?>(request.WorkflowName, request);
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken token)
    {
        return _context.CreateTimer(delay, token);
    }

    private async Task ExecuteRequestAsync(IRequest request, TaskOptions? taskOptions)
    {
        await _context.CallDurableMediatorAsync(new MediatorRequest(request), taskOptions);
    }

    private async Task<TResponse> ExecuteRequestAsync<TResponse>(IRequest<TResponse> request, TaskOptions? taskOptions)
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

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }

    private static TaskOptions DefaultTaskOptions(int maxAttempts, TimeSpan? delay)
        => new(new TaskRetryOptions(new RetryPolicy(maxAttempts, DelayOrDefault(delay), backoffCoefficient: 2)));

    private static TimeSpan DelayOrDefault(TimeSpan? delay)
        => delay ?? TimeSpan.FromMilliseconds(Random.Shared.Next(500, 800));
}
