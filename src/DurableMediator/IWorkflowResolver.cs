using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal interface IWorkflowResolver
{
    IWorkflowWrapper GetWorkflow(string workflowRequestName);

    [Obsolete]
    EntityId GetEntityId(IDurableOrchestrationContext context);
}
