﻿using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

public record WorkflowExecution<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    EntityId EntityId,
    IDurableMediator DurableMediator,
    ILogger ReplaySafeLogger) : IWorkflowExecution, ISubWorkflowOrchestrator
{
    public async Task<TResponse> ExecuteAsync<TResponse>(IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await DurableMediator.SendObjectAsync(new MediatorRequest(
                CurrentInput.Tracing,
                WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId), 
                (IRequest<Unit>)request));

            return default!;
        }

        var response = await DurableMediator.SendObjectWithResponseAsync(new MediatorRequestWithResponse(
            CurrentInput.Tracing,
            WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId), 
            (IRequest<object>)request));

        if (response == null)
        {
            throw new Exception("Received an empty response");
        }

        return (TResponse)(response.Response);
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

            ReplaySafeLogger.LogInformation("Execution attempt {attempt} of {requestName} failed", attempt, request.GetType().Name);

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
                    ReplaySafeLogger.LogInformation(ex, "Execution attempt {attempt} of {requestName} failed", attempt, request.GetType().Name);
                }
            }
            else
            {
                ReplaySafeLogger.LogInformation("Execution attempt {attempt} of {requestName} skipped due to check failure", attempt, request.GetType().Name);
            }

            await OrchestrationContext.CreateTimer(
                DateTime.UtcNow.Add(delay * attempt),
                token);

            try
            {
                var result = await ExecuteAsync(checkIfRequestApplied);

                if (result != null)
                {
                    ReplaySafeLogger.LogInformation("Execution attempt {attempt} of {requestName} check found successful execution", attempt, request.GetType().Name);

                    return result;
                }

                checkFailed = false;
            }
            catch (Exception ex)
            {
                ReplaySafeLogger.LogInformation(ex, "Execution attempt {attempt} of {requestName} check failed", attempt, request.GetType().Name);

                checkFailed = true;
            }
        }
        while (attempt < maxAttempts);

        throw new OrchestrationRetryException();
    }


    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.CallSubOrchestratorAsync<TWorkflowResponse?>(request.GetType().Name, WorkflowInstanceIdHelper.GetId(request), ForwardRequestWrapper(request));

    public void StartWorkflow(IWorkflowRequest request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, ForwardRequestWrapper(request), WorkflowInstanceIdHelper.GetId(request));

    public void StartWorkflow<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, ForwardRequestWrapper(request), WorkflowInstanceIdHelper.GetId(request));

    private WorkflowRequestWrapper<TSubWorkflowRequest> ForwardRequestWrapper<TSubWorkflowRequest>(TSubWorkflowRequest request) 
        => new WorkflowRequestWrapper<TSubWorkflowRequest>(CurrentInput.Tracing, request);

    private WorkflowRequestWrapper<TRequest> CurrentInput 
        => OrchestrationContext.GetInput<WorkflowRequestWrapper<TRequest>>();
}
