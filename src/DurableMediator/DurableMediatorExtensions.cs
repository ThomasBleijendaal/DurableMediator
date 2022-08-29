using MediatR;

namespace DurableMediator;

public static class DurableMediatorExtensions
{
    public static async Task<TResponse> SendAsync<TResponse>(this IWorkflowContext context, IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await context.DurableMediator.SendObjectAsync(new MediatorRequest((IRequest<Unit>)request));

            return default!;
        }

        var response = await context.DurableMediator.SendObjectWithResponseAsync(new MediatorRequestWithResponse((IRequest<object>)request));

        return (TResponse)(response?.Response ?? throw new Exception("Received an empty response"));
    }

    public static async Task SendWithRetryAsync(
        this IWorkflowContext context,
        IRequest<IRetryResponse> request,
        CancellationToken token,
        int maxRetries = 3,
        int millisecondsBetweenAttempt = 1000)
    {
        var attempt = 0;
        do
        {
            attempt++;

            var result = await context.SendAsync(request);

            if (result.IsSuccess)
            {
                return;
            }

            context.

            await context.OrchestrationContext.CreateTimer(
                DateTime.UtcNow.AddMilliseconds(millisecondsBetweenAttempt * attempt),
                token);
        }
        while (attempt < maxRetries);

        throw new OrchestrationRetryException();
    }
}
