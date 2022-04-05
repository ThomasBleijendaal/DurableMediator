namespace DurableMediator;

public interface IWorkflowMonitor
{
    Task<WorkflowStatus> GetWorkflowAsync(string instanceId);

    Task<IReadOnlyList<WorkflowStatus>> GetRecentWorkflowsAsync(string instanceIdPrefix);

    Task<bool> HasRunningTaskAsync(string instanceIdPrefix);
}
