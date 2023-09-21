using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp;

public static class BasicWorkflowTrigger
{
    [Function(nameof(StartHelloCitiesTypedAsync))]
    public static async Task<HttpResponseData> StartHelloCitiesTypedAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "basic")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        var start = await client.ScheduleNewBasicWorkflowInstanceAsync(new BasicWorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(start);

        return response;
    }
}
