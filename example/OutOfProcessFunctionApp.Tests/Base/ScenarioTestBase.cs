using DurableMediator.OutOfProcess;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;

namespace OutOfProcessFunctionApp.Tests.Base;

public abstract class ScenarioTestBase
{
    public abstract Task ScenarioAsync(Scenario scenario);

    public Task TestScenarioAsync<TWorkflow, TWorkflowRequest, TWorkflowResponse>(TWorkflow workflow, Scenario scenario)
        where TWorkflow : Workflow<TWorkflowRequest, TWorkflowResponse>
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>
        => TestScenarioAsync<TWorkflowRequest, TWorkflowResponse>(workflow.OrchestrateAsync, scenario);

    public Task TestScenarioAsync<TWorkflow, TWorkflowRequest>(TWorkflow workflow, Scenario scenario)
        where TWorkflow : Workflow<TWorkflowRequest>
        where TWorkflowRequest : IWorkflowRequest
        => TestScenarioAsync<TWorkflowRequest, Unit>(workflow.OrchestrateAsync, scenario);

    private static async Task TestScenarioAsync<TWorkflowRequest, TWorkflowResponse>(Func<IWorkflowExecution<TWorkflowRequest>, Task> workflow, Scenario scenario)
    {
        var orchestrationContextMock = new Mock<TaskOrchestrationContext>();

        var executionMock = new Mock<IWorkflowExecution<TWorkflowRequest>>();
        executionMock.SetupGet(x => x.ReplaySafeLogger).Returns(Mock.Of<ILogger>());
        executionMock.SetupGet(x => x.Request).Returns((TWorkflowRequest)scenario.Request);
        executionMock.SetupGet(x => x.OrchestrationContext).Returns(orchestrationContextMock.Object);

        var setup = new ScenarioSetup<TWorkflowRequest>(executionMock);

        scenario.Setup(setup, orchestrationContextMock);

        var scenarioRun = new ScenarioRun();

        var scenarioEnumerator = scenario.RunScenario(scenarioRun).GetEnumerator();

        var scenarioChecker = new ScenarioChecker<TWorkflowRequest>(scenarioRun, executionMock.Object, scenarioEnumerator);

        try
        {
            var resultTask = workflow.Invoke(scenarioChecker);

            await resultTask;

            if (typeof(TWorkflowResponse) != typeof(Unit))
            {
                if (!resultTask.IsCompletedSuccessfully || resultTask is not Task<TWorkflowResponse> responseTask)
                {
                    Assert.Fail($"Incorrect output at end of workflow (at invocation {scenarioChecker.Invocations})");
                }
                else
                {
                    scenarioChecker.CheckRequest(new Scenario.Output(responseTask.Result));
                }
            }
        }
        catch (Exception ex) when (ex is not AssertionException)
        {
            scenarioChecker.CheckRequest(new Scenario.Throws(ex.GetType()));
        }

        if (scenarioEnumerator.MoveNext())
        {
            Assert.Fail($"Scenario expected more requests than workflow produced (at invocation {scenarioChecker.Invocations})");
        }
    }
}
