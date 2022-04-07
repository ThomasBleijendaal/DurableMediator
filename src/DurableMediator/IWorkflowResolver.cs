using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal interface IWorkflowResolver
{
    IWorkflowWrapper GetWorkflow(string workflowRequestName);

    EntityId GetEntityId(IDurableOrchestrationContext context);
}
