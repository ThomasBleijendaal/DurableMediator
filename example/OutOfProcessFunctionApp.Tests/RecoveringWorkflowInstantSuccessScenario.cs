using DurableMediator.OutOfProcess;
using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Tests;

public class RecoveringWorkflowInstantSuccessScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override IWorkflowRequest Request => new RecoveringWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        Setup.SetupRequestWithCheck<FailingRequest, CheckIfFailingRequestAppliedRequest, BasicResponse>(new BasicResponse(Guid.NewGuid()));

        yield return new CheckRequest(
            new FailingRequest(_requestId),
            new CheckIfFailingRequestAppliedRequest(_requestId),
            20,
            null);
    }
}
