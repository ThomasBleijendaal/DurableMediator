namespace DurableMediator;

/// <summary>
/// This interface is used as the durable entity proxy interface. This limits the use of generics (to none) and number of arguments (to one) that each of these methods is allowed to have.
/// 
/// Use the DurableMediatorExtensions for more useful API.
/// </summary>
public interface IDurableMediator
{
    Task SendObjectAsync(WorkflowRequest request);
    Task<WorkflowResponse> SendObjectWithResponseAsync(WorkflowRequestWithResponse request);
}
