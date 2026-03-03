using DurableMediator.OutOfProcess;
using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Tests;

public class RecoveringWorkflowFailingScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override IWorkflowRequest Request => new RecoveringWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        Setup.SetupRequestWithCheckThrows<FailingRequest, CheckIfFailingRequestAppliedRequest, BasicResponse>(
            new InvalidOperationException("FailingRequest failed"));

        yield return new CheckRequest(
            new FailingRequest(_requestId),
            new CheckIfFailingRequestAppliedRequest(_requestId),
            20,
            null);

        yield return new Throws(typeof(InvalidOperationException));
    }
}
