using System.Net;
using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp;

public class BasicSyncWorkflowTrigger
{
    private readonly ISyncWorkflowClient _client;

    public BasicSyncWorkflowTrigger(
        ISyncWorkflowClient syncWorkflowClient)
    {
        _client = syncWorkflowClient;
    }

    [Function(nameof(BasicSyncWorkflowTrigger))]
    public async Task<HttpResponseData> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basic/sync")] HttpRequestData req)
    {
        // the sync workflow client allows for executing workflows outside of the durable function context
        await _client.RunWorkflowAsync(new BasicWorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.WriteString("Done");

        return response;
    }
}
