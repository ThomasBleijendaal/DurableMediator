namespace DurableMediator.OutOfProcess.Testing;

public interface IScenarioRun
{
    /// <summary>
    /// The CurrentUtcDateTime that is the same as TaskOrchestrationContext.CurrentUtcDateTime in the workflow.
    /// </summary>
    DateTime CurrentUtcDateTime { get; }

    /// <summary>
    /// Guid generator that generates the same Guids as TaskOrchestrationContext.NextGuid() generates in the workflow.
    /// </summary>
    /// <returns></returns>
    Guid NewGuid();
}
