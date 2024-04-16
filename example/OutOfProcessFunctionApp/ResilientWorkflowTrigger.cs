using System.Net;
using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        var start = await client.StartWorkflowAsync(new ResilientWorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(start);

        return response;
    }
}
