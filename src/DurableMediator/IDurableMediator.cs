using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

/// <summary>
/// This interface is used as the durable entity proxy interface. This limits the use of generics (to none) and number of arguments (to one) that each of these methods is allowed to have.
/// </summary>
public interface IDurableMediator
{
    Task SendObjectAsync(MediatorRequest request);
    Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request);
}
