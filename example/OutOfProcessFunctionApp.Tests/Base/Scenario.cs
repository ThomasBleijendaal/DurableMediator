using System.Collections;
using DurableMediator.OutOfProcess;
using Microsoft.DurableTask;
using Moq;

namespace OutOfProcessFunctionApp.Tests.Base;

public abstract class Scenario : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new TestCaseData(this).SetName($"{nameof(ScenarioTestBase.ScenarioAsync)}({GetType().Name})");
    }

    public abstract void Setup(IScenarioSetup scenarioSetup, Mock<TaskOrchestrationContext> taskOrchestrationContextMock);

    public abstract IWorkflowRequest Request { get; }

    public abstract IEnumerable<object> RunScenario(IScenarioRun scenarioRun);

    public record CreateTimer(DateTime DateTime);

    public record CheckRequest(object Request, object Check, int MaxAttemps, TimeSpan? Delay);

    public record Throws(Type Exception);

    public record Output(object? Result);
}
