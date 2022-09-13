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
        var recentWorkflow = await monitor.GetRecentWorkflowsAsync("", CancellationToken.None).ToListAsync(100);

        return new OkObjectResult(recentWorkflow);
    }

    private static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, int count)
    {
        var list = new List<T>();

        await foreach (var item in enumerable)
        {
            list.Add(item);

            if (list.Count >= count)
            {
                break;
            }
        }

        return list;
    }
}
