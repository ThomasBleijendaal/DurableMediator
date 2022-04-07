namespace DurableMediator;

internal class WorkflowInstanceIdHelper
{
    public static string GetId<TResponse>(IWorkflowRequest<TResponse> request)
        => Constants.WorkflowIdPrefix + request.InstanceId;
}
