using System.Text.Json;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

public class OrchestratorExecutor<TWorkflowRequest, TWorkflowResponse> : IWorkflowExecution<TWorkflowRequest>
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

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await _context.CallDurableMediatorAsync(new MediatorRequest((IRequest<Unit>)request));

            return default!;
        }

        var response = await _context.CallDurableMediatorWithResponseAsync(new MediatorRequestWithResponse(request));

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }

    public async Task<TResponse> SendWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null)
    {
        var response = await _context.CallDurableMediatorWithCheckAndResponseAsync(
            new MediatorRequestWithCheckAndResponse(request, checkIfRequestApplied),
            new TaskOptions(
                new TaskRetryOptions(
                    new RetryPolicy(maxAttempts, DelayOrDefault(delay), backoffCoefficient: 2))));

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }

    public Task<TSubWorkflowResponse?> CallSubWorkflowAsync<TSubWorkflowResponse>(IWorkflowRequest<TSubWorkflowResponse> request)
    {
        return _context.CallSubOrchestratorAsync<TSubWorkflowResponse?>(request.WorkflowName, request);
    }

    private static TimeSpan DelayOrDefault(TimeSpan? delay)
        => delay ?? TimeSpan.FromMilliseconds(Random.Shared.Next(500, 800));
}
