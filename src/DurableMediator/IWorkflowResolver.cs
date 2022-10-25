namespace DurableMediator;

internal interface IWorkflowResolver
{
    IWorkflowWrapper GetWorkflow(string workflowRequestName);
}
