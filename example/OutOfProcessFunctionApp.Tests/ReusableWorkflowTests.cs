using OutOfProcessFunctionApp.Tests.Base;
using OutOfProcessFunctionApp.Workflows;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Tests;

public class ReusableWorkflowTests : ScenarioTestBase
{
    private ReusableWorkflow _subject;

    [SetUp]
    public void Setup()
    {
        _subject = new ReusableWorkflow();
    }

    [TestCaseSource(typeof(ReusableWorkflowScenario))]
    public override async Task ScenarioAsync(Scenario scenario)
    {
        await TestScenarioAsync<ReusableWorkflow, ReusableWorkflowRequest, ReusableWorkflowResponse>(_subject, scenario);
    }
}
