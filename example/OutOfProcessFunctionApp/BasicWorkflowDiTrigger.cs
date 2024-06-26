﻿using System.Net;
using DurableMediator.OutOfProcess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp;

public class BasicWorkflowDiTrigger
{
    private readonly IWorkflowStarter _workflowStarter;

    public BasicWorkflowDiTrigger(
        IWorkflowStarter workflowStarter)
    {
        _workflowStarter = workflowStarter;
    }

    [Function(nameof(BasicWorkflowDiTrigger))]
    public async Task<HttpResponseData> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "basic/di")] HttpRequestData req)
    {
        var start = await _workflowStarter.StartWorkflowAsync(new BasicWorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(start);

        return response;
    }
}
