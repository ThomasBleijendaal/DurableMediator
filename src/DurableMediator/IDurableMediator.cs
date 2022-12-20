namespace DurableMediator;

public interface IDurableMediator
{
    /// <summary>
    /// Do not use this interface
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task SendObjectAsync(MediatorRequest request);

    /// <summary>
    /// Do not use this interface
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request);

    /// <summary>
    /// Do not use this interface
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<MediatorResponse> SendObjectWithCheckAndResponseAsync(MediatorRequestWithCheckAndResponse request);
}
