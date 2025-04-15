using System.Collections;
using DurableTask.Core;
using Moq;
using NUnit.Framework;

namespace DurableMediator.HostedService.Testing;

public abstract class Scenario : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new TestCaseData(this).SetName($"{nameof(ScenarioTestBase.ScenarioAsync)}({GetType().Name})");
    }

    public abstract void Setup(IScenarioSetup scenarioSetup, Mock<OrchestrationContext> orchestrationContextMock);

    public abstract IWorkflowRequest Request { get; }

    public abstract IEnumerable<object> RunScenario(IScenarioRun scenarioRun);

    public record CreateTimer(DateTime DateTime);

    public record CreateDelay(TimeSpan Delay);

    public record CheckRequest(object Request, object Check, int MaxAttemps, TimeSpan? Delay);

    public record Throws(Type Exception);

    public record Output(object? Result);
}
