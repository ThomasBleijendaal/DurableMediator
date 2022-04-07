namespace DurableMediator;

public interface IWorkflowMonitor
{
    Task<WorkflowStatus?> GetWorkflowAsync(string instanceId);

    Task<TResult?> GetWorkflowResultAsync<TRequest, TResult>(string instanceId)
        where TRequest : IWorkflowRequest<TResult>;

    Task<IReadOnlyList<WorkflowStatus>> GetRecentWorkflowsAsync(string instanceIdPrefix, CancellationToken token);

    Task<bool> HasRunningTaskAsync(string instanceIdPrefix, CancellationToken token);
}
