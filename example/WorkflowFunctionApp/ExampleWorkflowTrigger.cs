using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

namespace WorkflowFunctionApp;

internal record ExampleWorkflowTrigger(
    IWorkflowOrchestrator<ExampleWorkflowRequest, ExampleWorkflow> WorkflowOrchestrator)
{
    [FunctionName(nameof(ExampleWorkflowTrigger))]
    public async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req)
    {
        var start = await WorkflowOrchestrator.StartNewAsync(new ExampleWorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", start);
    }
}
