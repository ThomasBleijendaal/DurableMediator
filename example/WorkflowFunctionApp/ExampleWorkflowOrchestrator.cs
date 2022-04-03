using DurableMediator;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

// TODO: autogen these orchestrators
internal record ExampleWorkflowOrchestrator(
    IWorkflowOrchestrator<ExampleWorkflowRequest, ExampleWorkflow> WorkflowOrchestrator)
{
    [FunctionName(nameof(ExampleWorkflow))]
    public Task OrchestrateAsync([OrchestrationTrigger] IDurableOrchestrationContext context) 
        => WorkflowOrchestrator.OrchestrateAsync(context);
}
