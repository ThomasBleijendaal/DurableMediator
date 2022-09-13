using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

public record WorkflowExecution<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    EntityId EntityId,
    IDurableMediator DurableMediator) : IWorkflowExecution, ISubWorkflowOrchestrator
{
    public async Task TryAsync(
        Func<Task<bool>> action,
        CancellationToken token,
        int maxRetries = 3,
        int millisecondsBetweenAttempt = 1000)
    {
        var attempt = 0;
        do
        {
            attempt++;

            if (await action.Invoke())
            {
                return;
            }

            await OrchestrationContext.CreateTimer(
                DateTime.UtcNow.AddMilliseconds(millisecondsBetweenAttempt * attempt),
                token);
        }
        while (attempt < maxRetries);

        throw new OrchestrationRetryException();
    }

    public async Task<TResponse> ExecuteAsync<TResponse>(IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await DurableMediator.SendObjectAsync(new MediatorRequest((IRequest<Unit>)request));

            return default!;
        }

        var response = await DurableMediator.SendObjectWithResponseAsync(new MediatorRequestWithResponse((IRequest<object>)request));

        return (TResponse)(response?.Response ?? throw new Exception("Received an empty response"));
    }

    public async Task ExecuteWithRetryAsync(
        IRequest<IRetryResponse> request,
        CancellationToken token,
        int maxRetries = 3,
        TimeSpan delay = default)
    {
        if (delay == default)
        {
            delay = TimeSpan.FromSeconds(1);
        }

        var attempt = 0;
        do
        {
            attempt++;

            var result = await ExecuteAsync(request);

            if (result.IsSuccess)
            {
                return;
            }

            await OrchestrationContext.CreateTimer(
                DateTime.UtcNow.Add(delay * attempt),
                token);
        }
        while (attempt < maxRetries);

        throw new OrchestrationRetryException();
    }

    public async Task<TResponse> ExecuteWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        TimeSpan delay = default)
    {
        if (delay == default)
        {
            delay = TimeSpan.FromSeconds(1);
        }

        await OrchestrationContext.CreateTimer(DateTime.UtcNow.Add(delay), token);

        return await ExecuteAsync(request);
    }

    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.CallSubOrchestratorAsync<TWorkflowResponse?>(request.GetType().Name, WorkflowInstanceIdHelper.GetId(request), request);

    public void StartWorkflow(IWorkflowRequest request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, request, WorkflowInstanceIdHelper.GetId(request));
}
