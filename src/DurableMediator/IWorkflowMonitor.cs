namespace DurableMediator;

public interface IWorkflowMonitor // TODO: 
{
    Task<WorkflowStatus> GetWorkflowAsync(string instanceId);

    Task<IReadOnlyList<WorkflowStatus>> GetRecentWorkflowsAsync(string instanceIdPrefix, CancellationToken token);

    Task<bool> HasRunningTaskAsync(string instanceIdPrefix, CancellationToken token);
}
