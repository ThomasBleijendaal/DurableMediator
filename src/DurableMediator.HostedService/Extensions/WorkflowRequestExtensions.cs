namespace DurableMediator.HostedService.Extensions;

internal static class WorkflowRequestExtensions
{
    public static IWorkflow GetWorkflow(this IWorkflowRequest request)
    {
        var requestType = request.GetType();
        var workflowTypeInfo = requestType.Assembly.DefinedTypes.FirstOrDefault(x => x.Name == request.WorkflowName)
            ?? throw new InvalidOperationException($"Cannot find type of workflow {request.WorkflowName}");

        var workflowType = requestType.Assembly.GetType(workflowTypeInfo.FullName
            ?? throw new InvalidOperationException($"Cannot find type of workflow {request.WorkflowName}"))
            ?? throw new InvalidOperationException($"Cannot find type of workflow {request.WorkflowName}");

        return Activator.CreateInstance(workflowType) as IWorkflow
            ?? throw new InvalidOperationException($"Cannot find type of workflow {request.WorkflowName}");
    }
}
