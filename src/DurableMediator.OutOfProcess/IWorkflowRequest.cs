namespace DurableMediator.OutOfProcess;

public interface IWorkflowRequest<TWorkflowResponse>
{
    static abstract Type Workflow { get; }
}
