using DurableMediator.OutOfProcess;
using DurableMediator.OutOfProcess.Testing;
using Microsoft.DurableTask;
using Moq;
using OutOfProcessFunctionApp.Workflows;
using WorkflowHandlers.Responses;

namespace OutOfProcessFunctionApp.Tests;

public class ReusableWorkflowScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override void Setup(IScenarioSetup scenarioSetup, Mock<TaskOrchestrationContext> taskOrchestrationContextMock)
    {
    }

    public override IWorkflowRequest Request => new ReusableWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        yield return new CreateTimer(scenarioRun.CurrentUtcDateTime.AddSeconds(10));
        yield return new Output(new ReusableWorkflowResponse("ReusableWorkflow"));
    }
}
