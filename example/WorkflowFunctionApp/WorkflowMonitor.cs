using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace WorkflowFunctionApp;

internal static class WorkflowMonitor
{
    [FunctionName(nameof(WorkflowMonitor))]
    public static async Task<IActionResult> MonitorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "monitor")] HttpRequestMessage req,
        [Workflow] IWorkflowMonitor monitor)
    {
        var runningWorkflows = await monitor.GetRecentWorkflowsAsync("", CancellationToken.None);

        return new OkObjectResult(runningWorkflows.OrderByDescending(x => x.CreateTime));
    }
}
