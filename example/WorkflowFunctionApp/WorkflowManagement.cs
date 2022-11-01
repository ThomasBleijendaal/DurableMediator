using DurableMediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace WorkflowFunctionApp;

internal static class WorkflowManagement
{
    [FunctionName("RestartWorkflow")]
    public static async Task<IActionResult> RestartAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "management/restart/{instanceId}")] HttpRequestMessage req,
        [Workflow] IWorkflowManagement management,
        string instanceId)
    {
        await management.RestartWorkflowAsync(instanceId);

        return new NoContentResult();
    }

    [FunctionName("RewindWorkflow")]
    public static async Task<IActionResult> RewindAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "management/rewind/{instanceId}")] HttpRequestMessage req,
        [Workflow] IWorkflowManagement management,
        string instanceId)
    {
        await management.RewindWorkflowAsync(instanceId);

        return new NoContentResult();
    }
}
