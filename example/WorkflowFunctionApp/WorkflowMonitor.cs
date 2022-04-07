using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using WorkflowFunctionApp.Workflows;

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

internal static class WorkflowResult
{ 
    [FunctionName(nameof(WorkflowResult))]
    public static async Task<IActionResult> ResultAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "result/{instanceId}")] HttpRequestMessage req,
        [Workflow] IWorkflowMonitor monitor,
        string instanceId)
    {
        var response = await monitor.GetWorkflowResultAsync<BBBWorkflowRequest, BBBWorkflowResponse>(instanceId);

        return response == null 
            ? new NotFoundResult()
            : new OkObjectResult(response);
    }
}
