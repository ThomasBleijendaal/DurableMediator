using DurableMediator.Functions;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator.Executions;

internal record EntityExecution<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    ILogger ReplaySafeLogger) : BaseExecution<TRequest>(OrchestrationContext), IWorkflowExecution<TRequest>
{
    private IDurableMediator Mediator 
        => OrchestrationContext.CreateEntityProxy<IDurableMediator>(
            new EntityId(nameof(DurableMediatorEntity), OrchestrationContext.InstanceId));

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => ExecuteRequestAsync(request, 1);

    public Task<TResponse> SendWithRetryAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null)
        => ExecuteRequestAsync(request, maxAttempts, delay);

    public async Task<TResponse> SendWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        TimeSpan? delay = null)
    {
        await OrchestrationContext.CreateTimer(OrchestrationContext.CurrentUtcDateTime.Add(delay ?? TimeSpan.FromSeconds(1)), token);

        return await SendAsync(request);
    }

    public async Task<TResponse> SendWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null)
    {
        // TODO: replace with Mediator
        var response = await OrchestrationContext.CallActivityWithRetryAsync<MediatorResponse>(
            ActivityFunction.SendObjectWithCheckAndResponse,
            new RetryOptions(DelayOrDefault(delay), maxAttempts)
            {
                BackoffCoefficient = 2
            },
            new MediatorRequestWithCheckAndResponse(
                CurrentInput.Tracing,
                WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId),
                request,
                checkIfRequestApplied));

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return (TResponse)response.Response;
    }

    private async Task<TResponse> ExecuteRequestAsync<TResponse>(
        IRequest<TResponse> request,
        // TODO: implement
        int maxAttempts,
        TimeSpan? delay = null)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await Mediator.SendObjectAsync(new MediatorRequest(
                CurrentInput.Tracing,
                WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId),
                (IRequest<Unit>)request));

            return default!;
        }

        var response = await Mediator.SendObjectWithResponseAsync(new MediatorRequestWithResponse(
            CurrentInput.Tracing,
            WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId),
            request));

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return (TResponse)response.Response;
    }
}
