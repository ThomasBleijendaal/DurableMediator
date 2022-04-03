namespace DurableMediator;

/// <summary>
/// This interface does not enforce any API but allows the workflow author to pinky-promise that the specified TResult is returned from the orchestration.
/// </summary>
/// <typeparam name="TResult"></typeparam>
// TODO: fix this
public interface IWorkflowResult<TResult>
{

}
