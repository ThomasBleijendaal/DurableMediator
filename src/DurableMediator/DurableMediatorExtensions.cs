using MediatR;

namespace DurableMediator;

public static class DurableMediatorExtensions
{
    public static async Task<TResponse> SendAsync<TResponse>(this IDurableMediator mediator, IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await mediator.SendObjectAsync(new MediatorRequest((IRequest<Unit>)request));

            return default!;
        }

        var response = await mediator.SendObjectWithResponseAsync(new MediatorRequestWithResponse((IRequest<object>)request));

        return (TResponse)(response?.Response ?? throw new Exception("Received an empty response"));
    }
}
