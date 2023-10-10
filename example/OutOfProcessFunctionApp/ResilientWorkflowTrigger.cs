using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp;

public static class ResilientWorkflowTrigger
{
    [Function(nameof(ResilientWorkflowTrigger))]
    public static async Task<HttpResponseData> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resilient")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        var start = await client.ScheduleNewResilientWorkflowInstanceAsync(new ResilientWorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(start);

        return response;
    }
}
