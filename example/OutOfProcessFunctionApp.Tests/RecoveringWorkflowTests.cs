using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;

namespace OutOfProcessFunctionApp.Tests;

public class RecoveringWorkflowTests : ScenarioTestBase
{
    private RecoveringWorkflow _subject;

    [SetUp]
    public void Setup()
    {
        _subject = new RecoveringWorkflow();
    }

    [TestCaseSource(typeof(RecoveringWorkflowInstantSuccessScenario))]
    [TestCaseSource(typeof(RecoveringWorkflowFailingScenario))]
    public override async Task ScenarioAsync(Scenario scenario)
    {
        await TestScenarioAsync<RecoveringWorkflow, RecoveringWorkflowRequest>(_subject, scenario);
    }
}
