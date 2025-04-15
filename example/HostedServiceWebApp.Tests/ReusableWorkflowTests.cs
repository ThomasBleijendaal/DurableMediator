using DurableMediator.HostedService.Testing;
using HostedServiceWebApp.Workflows;
using WorkflowHandlers.Responses;

namespace HostedServiceWebApp.Tests;

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
