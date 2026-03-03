namespace DurableMediator.HostedService;

public interface IWorkflowEvent
{
    /// <summary>
    /// Name of the event raised in the corresponding workflow.
    /// </summary>
    string EventName { get; }

    /// <summary>
    /// Instance id of the workflow this event is raised for.
    /// </summary>
    string InstanceId { get; }
}

/// <summary>
/// Workflow event that can start the corresponding workflow when it's not running yet.
/// </summary>
/// <typeparam name="TWorkflowRequest"></typeparam>
public interface IWorkflowEvent<TWorkflowRequest> : IWorkflowEvent
{
    /// <summary>
    /// Workflow request to start the workflow if this event is raised before the workflow runs.
    /// </summary>
    /// <returns></returns>
    TWorkflowRequest? GetWorkflowRequest();
}
