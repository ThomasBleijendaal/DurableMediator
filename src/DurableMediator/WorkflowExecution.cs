﻿using DurableMediator.Functions;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

public record WorkflowExecution<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    ILogger ReplaySafeLogger) : IWorkflowExecution, ISubWorkflowOrchestrator
{
    public Task<TResponse> ExecuteAsync<TResponse>(IRequest<TResponse> request) 
        => ExecuteRequestAsync(request, 1);


    public Task<TResponse> ExecuteWithRetryAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null)
        => ExecuteRequestAsync(request, maxAttempts, delay);
    //{
      //  return 

        //if (delay == default)
        //{
        //    delay = TimeSpan.FromSeconds(1);
        //}

        //var attempt = 0;
        //do
        //{
        //    attempt++;

        //    var result = await ExecuteAsync(request);

        //    if (result.IsSuccess)
        //    {
        //        return;
        //    }

        //    ReplaySafeLogger.LogInformation("Execution attempt {attempt} of {requestName} failed", attempt, request.GetType().Name);

        //    await OrchestrationContext.CreateTimer(
        //        DateTime.UtcNow.Add(delay * attempt),
        //        token);
        //}
        //while (attempt < maxAttempts);

        //throw new OrchestrationRetryException();
    //}

    public async Task<TResponse> ExecuteWithDelayAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken token,
        TimeSpan? delay = null)
    {
        await OrchestrationContext.CreateTimer(DateTime.UtcNow.Add(delay ?? TimeSpan.FromSeconds(1)), token);

        return await ExecuteAsync(request);
    }

    public async Task<TResponse> ExecuteWithCheckAsync<TResponse>(
        IRequest<TResponse> request,
        IRequest<TResponse?> checkIfRequestApplied,
        CancellationToken token,
        int maxAttempts = 3,
        TimeSpan? delay = null)
    {
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
                DateTime.UtcNow.Add((delay ?? TimeSpan.FromSeconds(1)) * attempt),
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

    private async Task<TResponse> ExecuteRequestAsync<TResponse>(
        IRequest<TResponse> request, 
        int maxAttempts,
        TimeSpan? delay = null)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await SendObjectAsync(request, maxAttempts, delay);

            return default!;
        }

        var response = await SendObjectWithResponseAsync(request, maxAttempts, delay);

        if (response == null)
        {
            throw new Exception("Received an empty response");
        }

        return (TResponse)response.Response;
    }

    private async Task SendObjectAsync<TResponse>(IRequest<TResponse> request, int maxAttempts, TimeSpan? delay) 
        => await OrchestrationContext.CallActivityWithRetryAsync(ActivityFunction.SendObject,
            new RetryOptions(DelayOrDefault(delay), maxAttempts)
            {
                BackoffCoefficient = 2
            },
            new MediatorRequest(
                CurrentInput.Tracing,
                WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId),
                (IRequest<Unit>)request));

    private async Task<MediatorResponse> SendObjectWithResponseAsync<TResponse>(IRequest<TResponse> request, int maxAttempts, TimeSpan? delay) 
        => await OrchestrationContext.CallActivityWithRetryAsync<MediatorResponse>(ActivityFunction.SendObjectWithResponse,
            new RetryOptions(DelayOrDefault(delay), maxAttempts)
            {
                BackoffCoefficient = 2
            },
            new MediatorRequestWithResponse(
                CurrentInput.Tracing,
                WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId),
                (IRequest<object>)request));

    private static TimeSpan DelayOrDefault(TimeSpan? delay)
        => delay ?? TimeSpan.FromMilliseconds(Random.Shared.Next(500, 800));
}
