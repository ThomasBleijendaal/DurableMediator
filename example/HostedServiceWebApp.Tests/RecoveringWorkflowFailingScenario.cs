﻿using DurableMediator.HostedService;
using DurableMediator.HostedService.Testing;
using DurableTask.Core;
using HostedServiceWebApp.Workflows;
using Moq;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace HostedServiceWebApp.Tests;

public class RecoveringWorkflowFailingScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override void Setup(IScenarioSetup scenarioSetup, Mock<OrchestrationContext> taskOrchestrationContextMock)
    {
        scenarioSetup.SetupRequestWithCheckThrows<FailingRequest, CheckIfFailingRequestAppliedRequest, BasicResponse>(
            new InvalidOperationException("FailingRequest failed"));
    }

    public override IWorkflowRequest Request => new RecoveringWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        yield return new CheckRequest(
            new FailingRequest(_requestId),
            new CheckIfFailingRequestAppliedRequest(_requestId),
            20,
            null);

        yield return new Throws(typeof(InvalidOperationException));
    }
}
