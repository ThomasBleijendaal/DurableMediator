using System.Net;
using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp;

public static class RecoveringWorkflowTrigger
{
    [Function(nameof(RecoveringWorkflowTrigger))]
    public static async Task<HttpResponseData> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recovering")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        var start = await client.StartWorkflowAsync(new RecoveringWorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(start);

        return response;
    }
}
