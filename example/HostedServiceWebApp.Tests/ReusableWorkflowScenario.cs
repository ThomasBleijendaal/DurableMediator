using DurableMediator.HostedService;
using DurableMediator.HostedService.Testing;
using HostedServiceWebApp.Workflows;
using WorkflowHandlers.Responses;

namespace HostedServiceWebApp.Tests;

public class ReusableWorkflowScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override IWorkflowRequest Request => new ReusableWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        yield return new CreateTimer(scenarioRun.CurrentUtcDateTime.AddSeconds(10));
        yield return new Output(new ReusableWorkflowResponse("ReusableWorkflow"));
    }
}
