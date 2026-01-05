using System.Net;
using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp;

public static class ExternalEventWorkflowTrigger
{
    [Function("Start" + nameof(ExternalEventWorkflowTrigger))]
    public static async Task<HttpResponseData> StartExternalEventWorkflowAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "startevent/{id:guid}")] HttpRequestData req,
        Guid id,
        [DurableClient] DurableTaskClient client)
    {
        var start = await client.StartWorkflowAsync(new ExternalEventWorkflowRequest(id));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(start);

        return response;
    }

    [Function("Signal" + nameof(ExternalEventWorkflowTrigger))]
    public static async Task<HttpResponseData> SignalExternalEventWorkflowAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "signalevent/{instanceId:guid}/{id}")] HttpRequestData req,
        Guid instanceId,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        await client.RaiseEventAsync(new ExternalEvent(id, instanceId, Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Created);

        return response;
    }
}
