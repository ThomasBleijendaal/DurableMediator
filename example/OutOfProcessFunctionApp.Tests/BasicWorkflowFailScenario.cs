using DurableMediator.OutOfProcess;
using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Tests;

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
