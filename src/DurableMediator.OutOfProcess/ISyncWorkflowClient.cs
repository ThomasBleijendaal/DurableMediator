namespace DurableMediator.OutOfProcess;

public interface ISyncWorkflowClient
{
    Task RunWorkflowAsync(IWorkflowRequest request);

    Task<TWorkflowResponse> RunWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request);
}
