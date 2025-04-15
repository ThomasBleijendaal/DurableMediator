using DurableMediator.HostedService;
using DurableMediator.HostedService.Testing;
using DurableTask.Core;
using HostedServiceWebApp.Workflows;
using Moq;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace HostedServiceWebApp.Tests;

public class BasicWorkflowScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override void Setup(IScenarioSetup scenarioSetup, Mock<OrchestrationContext> taskOrchestrationContextMock)
    {
        scenarioSetup.SetupRequest<SimpleRequest, BasicResponse>(new BasicResponse(Guid.NewGuid()));
        scenarioSetup.SetupCallSubWorkflow<ReusableWorkflowRequest, ReusableWorkflowResponse>(new ReusableWorkflowResponse("Done"));
    }

    public override IWorkflowRequest Request => new BasicWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        yield return new CommandRequest(_requestId, "command");
        yield return new SimpleRequest(_requestId, "1");
        yield return new SimpleRequest(_requestId, "2");
        yield return new SimpleRequest(_requestId, "3");
        yield return new SimpleRequest(_requestId, "A");
        yield return new SimpleRequest(_requestId, "B");
        yield return new SimpleRequest(_requestId, "C");
        yield return new SimpleRequest(_requestId, "D");
        yield return new SimpleRequest(_requestId, "E");
        yield return new SimpleRequest(_requestId, "F");
        yield return new SimpleRequest(_requestId, "G");
        yield return new SimpleRequest(_requestId, "H");
        yield return new SimpleRequest(_requestId, "I");
        yield return new SimpleRequest(_requestId, "J");
        yield return new SimpleRequest(_requestId, "K");
        yield return new SimpleRequest(_requestId, "L");
        yield return new SimpleRequest(_requestId, "M");
        yield return new SimpleRequest(_requestId, "N");
        yield return new SimpleRequest(_requestId, "O");
        yield return new SimpleRequest(_requestId, "P");
        yield return new SimpleRequest(_requestId, "Q");
        yield return new SimpleRequest(_requestId, "R");
        yield return new SimpleRequest(_requestId, "S");
        yield return new SimpleRequest(_requestId, "T");
        yield return new SimpleRequest(_requestId, "U");
        yield return new SimpleRequest(_requestId, "V");
        yield return new SimpleRequest(_requestId, "W");
        yield return new SimpleRequest(_requestId, "X");
        yield return new SimpleRequest(_requestId, "Y");
        yield return new SimpleRequest(_requestId, "Z");
        yield return new SlowRequest(_requestId);
        yield return new CreateDelay(TimeSpan.FromSeconds(1));
        yield return new ReusableWorkflowRequest(scenarioRun.NewGuid());
    }
}
