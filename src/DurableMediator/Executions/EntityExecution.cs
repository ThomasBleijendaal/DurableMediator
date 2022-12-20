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
            new EntityId(nameof(DurableMediator), OrchestrationContext.InstanceId));

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
        var response = await RetryAsync(() => Mediator.SendObjectWithCheckAndResponseAsync(
            new MediatorRequestWithCheckAndResponse(
                CurrentInput.Tracing,
                OrchestrationContext.InstanceId,
                request,
                checkIfRequestApplied)),
            maxAttempts, delay);

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return (TResponse)response.Response;
    }

    private async Task<TResponse> ExecuteRequestAsync<TResponse>(
        IRequest<TResponse> request,
        int maxAttempts,
        TimeSpan? delay = null)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await RetryAsync(async () =>
            {
                await Mediator.SendObjectAsync(
                    new MediatorRequest(
                        CurrentInput.Tracing,
                        OrchestrationContext.InstanceId,
                        (IRequest<Unit>)request));

                return Unit.Value;
            }, 
            maxAttempts, delay);

            return default!;
        }

        var response = await RetryAsync(() => Mediator.SendObjectWithResponseAsync(
            new MediatorRequestWithResponse(
                CurrentInput.Tracing,
                OrchestrationContext.InstanceId,
                request)), 
            maxAttempts, delay);

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return (TResponse)response.Response;
    }

    private async Task<TResult> RetryAsync<TResult>(Func<Task<TResult>> action, int maxAttempts, TimeSpan? delay)
    {
        List<Exception>? exceptions = null;

        var attempt = 0;
        do
        {
            try
            {
                return await action.Invoke();
            }
            catch (Exception ex)
            {
                exceptions ??= new();
                exceptions.Add(ex);

                if (++attempt < maxAttempts)
                {
                    await OrchestrationContext.CreateTimer(OrchestrationContext.CurrentUtcDateTime.Add(delay ?? TimeSpan.FromSeconds(1)), CancellationToken.None);
                }
                else
                {
                    break;
                }
            }
        } while (true);

        throw new AggregateException("Failed to execute step", exceptions ?? Enumerable.Empty<Exception>());
    }
}
