using DurableMediator.HostedService.Testing;
using HostedServiceWebApp.Workflows;
using OutOfProcessFunctionApp.Tests;

namespace HostedServiceWebApp.Tests;

public class ExternalEventWorkflowTests : ScenarioTestBase
{
    private ExternalEventWorkflow _subject;

    [SetUp]
    public void Setup()
    {
        _subject = new ExternalEventWorkflow();
    }

    [TestCaseSource(typeof(ExternalEventWorkflowScenario))]
    [TestCaseSource(typeof(ExternalEventWorkflowTimeoutScenario))]
    [TestCaseSource(typeof(ExternalEventWorkflowSecondTimeoutScenario))]
    public override async Task ScenarioAsync(Scenario scenario)
    {
        await TestScenarioAsync<ExternalEventWorkflow, ExternalEventWorkflowRequest>(_subject, scenario);
    }
}
