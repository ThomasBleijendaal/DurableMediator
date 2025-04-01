namespace DurableMediator.HostedService;

public interface IWorkflowRequest<TWorkflowResponse> : IWorkflowRequest
{
}

public interface IWorkflowRequest
{
    string WorkflowName { get; }

    string? InstanceId { get; }
}
