using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp.Tests;

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
