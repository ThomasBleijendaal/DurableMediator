using DurableMediator.Functions;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

public record WorkflowExecution<TRequest>(
    TRequest Request,
    IDurableOrchestrationContext OrchestrationContext,
    ILogger ReplaySafeLogger) : IWorkflowExecution<TRequest>, ISubWorkflowOrchestrator
{
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
            await SendObjectAsync(ActivityFunction.SendObject, (IRequest<Unit>)request, maxAttempts, delay);

            return default!;
        }

        var response = await SendObjectAsync(ActivityFunction.SendObjectWithResponse, request, maxAttempts, delay);

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return (TResponse)response.Response;
    }

    private async Task<MediatorResponse> SendObjectAsync<TResponse>(string activity, IRequest<TResponse> request, int maxAttempts, TimeSpan? delay)
        => await OrchestrationContext.CallActivityWithRetryAsync<MediatorResponse>(activity,
            new RetryOptions(DelayOrDefault(delay), maxAttempts)
            {
                BackoffCoefficient = 2
            },
            new MediatorRequestWithResponse(
                CurrentInput.Tracing,
                WorkflowInstanceIdHelper.GetOriginalInstanceId(OrchestrationContext.InstanceId),
                request));

    private static TimeSpan DelayOrDefault(TimeSpan? delay)
        => delay ?? TimeSpan.FromMilliseconds(Random.Shared.Next(500, 800));
}
