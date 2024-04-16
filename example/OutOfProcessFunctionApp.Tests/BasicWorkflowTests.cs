﻿using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp.Tests;

public class BasicWorkflowTests : ScenarioTestBase
{
    private BasicWorkflow _subject;

    [SetUp]
    public void Setup()
    {
        _subject = new BasicWorkflow();
    }

    [TestCaseSource(typeof(BasicWorkflowScenario))]
    [TestCaseSource(typeof(BasicWorkflowFailScenario))]
    public override async Task ScenarioAsync(Scenario scenario)
    {
        await TestScenarioAsync<BasicWorkflow, BasicWorkflowRequest>(_subject, scenario);
    }
}
