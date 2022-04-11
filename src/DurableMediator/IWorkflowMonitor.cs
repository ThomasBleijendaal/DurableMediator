namespace DurableMediator;

public interface IWorkflowMonitor
{
    Task<WorkflowStatus?> GetWorkflowAsync(string instanceId);

    Task<(TRequest request, TResponse? response)> GetWorkflowDataAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>;

    IAsyncEnumerable<WorkflowStatus> GetRecentWorkflowsAsync(string instanceIdPrefix, CancellationToken token);

    Task<bool> HasRunningTaskAsync(string instanceIdPrefix, CancellationToken token);
}
