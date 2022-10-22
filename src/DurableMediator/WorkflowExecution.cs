using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

public record WorkflowExecution<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    EntityId EntityId,
    IDurableMediator DurableMediator,
    ILogger Logger) : IWorkflowExecution, ISubWorkflowOrchestrator
{
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
        int maxAttempts = 3,
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

            OrchestrationContext.CreateReplaySafeLogger(Logger).LogInformation("Execution attempt {attempt} failed", attempt);

            await OrchestrationContext.CreateTimer(
                DateTime.UtcNow.Add(delay * attempt),
                token);
        }
        while (attempt < maxAttempts);

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

    public async Task<TResponse> ExecuteWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan delay = default)
    {
        if (delay == default)
        {
            delay = TimeSpan.FromSeconds(1);
        }

        var attempt = 0;
        var checkFailed = false;
        do
        {
            attempt++;

            if (!checkFailed)
            {
                try
                {
                    var result = await ExecuteAsync(request);

                    return result;
                }
                catch (Exception ex)
                {
                    OrchestrationContext.CreateReplaySafeLogger(Logger).LogInformation(ex, "Execution attempt {attempt} failed", attempt);
                }
            }
            else
            {
                OrchestrationContext.CreateReplaySafeLogger(Logger).LogInformation("Execution attempt {attempt} skipped due to check failure", attempt);
            }

            await OrchestrationContext.CreateTimer(
                DateTime.UtcNow.Add(delay * attempt),
                token);

            try
            {
                var result = await ExecuteAsync(checkIfRequestApplied);

                if (result != null)
                {
                    OrchestrationContext.CreateReplaySafeLogger(Logger).LogInformation("Execution attempt {attempt} check found successful execution", attempt);

                    return result;
                }

                checkFailed = false;
            }
            catch (Exception ex)
            {
                OrchestrationContext.CreateReplaySafeLogger(Logger).LogInformation(ex, "Execution attempt {attempt} check failed", attempt);

                checkFailed = true;
            }
        }
        while (attempt < maxAttempts);

        throw new OrchestrationRetryException();
    }


    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.CallSubOrchestratorAsync<TWorkflowResponse?>(request.GetType().Name, WorkflowInstanceIdHelper.GetId(request), request);

    public void StartWorkflow(IWorkflowRequest request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, request, WorkflowInstanceIdHelper.GetId(request));

    public void StartWorkflow<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, request, WorkflowInstanceIdHelper.GetId(request));
}
