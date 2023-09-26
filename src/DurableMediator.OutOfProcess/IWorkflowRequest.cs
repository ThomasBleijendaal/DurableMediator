namespace DurableMediator.OutOfProcess;

public interface IWorkflowRequest<TWorkflowResponse> : IWorkflowRequest
{
}

public interface IWorkflowRequest
{
    string WorkflowName { get; }
}
