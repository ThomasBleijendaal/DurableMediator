using DurableMediator.HostedService;
using DurableMediator.HostedService.Testing;
using HostedServiceWebApp.Workflows;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace HostedServiceWebApp.Tests;

public class BasicWorkflowFailScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override IWorkflowRequest Request => new BasicWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        Setup.SetupRequestThrows<SimpleRequest, BasicResponse>(new InvalidOperationException());

        yield return new CommandRequest(_requestId, "command");
        yield return new SimpleRequest(_requestId, "1");
        yield return new Throws(typeof(InvalidOperationException));
    }
}
