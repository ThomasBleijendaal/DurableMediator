using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace WorkflowFunctionApp;

internal static class WorkflowResult
{ 
    [FunctionName(nameof(WorkflowResult))]
    public static async Task<IActionResult> ResultAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "result/{instanceId}")] HttpRequestMessage req,
        [Workflow] IWorkflowMonitor monitor,
        string instanceId)
    {
        var data = await monitor.GetWorkflowAsync(instanceId);

        return new OkObjectResult(data);
    }
}
