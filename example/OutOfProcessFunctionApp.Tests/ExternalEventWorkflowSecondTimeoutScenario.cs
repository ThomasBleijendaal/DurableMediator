using DurableMediator.OutOfProcess;
using DurableMediator.OutOfProcess.Testing;
using OutOfProcessFunctionApp.Workflows;
using WorkflowHandlers.Requests;

namespace OutOfProcessFunctionApp.Tests;

public class ExternalEventWorkflowSecondTimeoutScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override IWorkflowRequest Request => new ExternalEventWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        Setup.SetupEvent("external-event-1", new ExternalEvent("1", _requestId, scenarioRun.NewGuid()));
        Setup.SetupEvent("external-event-2", default(ExternalEvent));
        Setup.SetupEvent("external-event-3", new ExternalEvent("3", _requestId, scenarioRun.NewGuid()));
        Setup.SetupEvent("external-event-4", new ExternalEvent("4", _requestId, scenarioRun.NewGuid()));
        Setup.SetupEvent("external-event-5", new ExternalEvent("5", _requestId, scenarioRun.NewGuid()));

        yield return new WaitForEvent<ExternalEvent>("external-event-1", TimeSpan.FromSeconds(1));
        yield return new WaitForEvent<ExternalEvent>("external-event-2", TimeSpan.FromSeconds(10));
        yield return new SimpleRequest(_requestId, "Event 2 did not fire");
        yield return new WaitForEvent<ExternalEvent>("external-event-3", TimeSpan.FromSeconds(60));
        yield return new WaitForEvent<ExternalEvent>("external-event-4", Timeout.InfiniteTimeSpan);
        yield return new WaitForEvent<ExternalEvent>("external-event-5", Timeout.InfiniteTimeSpan);
    }
}
