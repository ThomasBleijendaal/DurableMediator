using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator.Executions;

internal record BaseExecution<TRequest>(
    IDurableOrchestrationContext OrchestrationContext)
{
    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.CallSubOrchestratorAsync<TWorkflowResponse?>(request.GetType().Name, request.InstanceId, ForwardRequestWrapper(request));

    public void StartWorkflow(IWorkflowRequest request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, ForwardRequestWrapper(request), request.InstanceId);

    public void StartWorkflow<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => OrchestrationContext.StartNewOrchestration(request.GetType().Name, ForwardRequestWrapper(request), request.InstanceId);

    protected WorkflowRequestWrapper<TRequest> CurrentInput
        => OrchestrationContext.GetInput<WorkflowRequestWrapper<TRequest>>();

    protected WorkflowRequestWrapper<TSubWorkflowRequest> ForwardRequestWrapper<TSubWorkflowRequest>(TSubWorkflowRequest request)
        => new WorkflowRequestWrapper<TSubWorkflowRequest>(CurrentInput.Tracing, request);

    protected static TimeSpan DelayOrDefault(TimeSpan? delay)
        => delay ?? TimeSpan.FromMilliseconds(Random.Shared.Next(500, 800));
}
