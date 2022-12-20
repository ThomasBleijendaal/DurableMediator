namespace DurableMediator;

public interface IDurableMediator
{
    Task SendObjectAsync(MediatorRequest request);
    Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request);
    Task<MediatorResponse> SendObjectWithCheckAndResponseAsync(MediatorRequestWithCheckAndResponse request);
}
