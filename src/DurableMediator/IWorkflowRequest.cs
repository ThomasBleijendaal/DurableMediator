namespace DurableMediator;

public interface IWorkflowRequest
{
    public string InstanceId { get; }

    // TODO: make generic?
    public Guid? AffectedEntityId { get; }
}
