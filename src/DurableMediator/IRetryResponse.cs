namespace DurableMediator;

/// <summary>
/// This interface can be used when the durable mediator must be able to verify whether a request
/// has been completed successfully.
/// </summary>
public interface IRetryResponse
{
    bool IsSuccess { get; }
}
