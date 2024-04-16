namespace DurableMediator.OutOfProcess;

public interface IWorkflowStarter
{
    Task<string> StartWorkflowAsync<TWorkflowRequest>(TWorkflowRequest workflowRequest)
        where TWorkflowRequest : IWorkflowRequest;
}
