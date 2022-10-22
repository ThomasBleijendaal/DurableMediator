using MediatR;

namespace DurableMediator;

internal class DurableMediatorEntity : IDurableMediator
{
    private readonly IMediator _mediator;

    public DurableMediatorEntity(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task SendObjectAsync(MediatorRequest request)
    {
        await _mediator.Send(request.Request).ConfigureAwait(false);
    }

    public async Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request)
    {
        try
        {
            // the dynamic is needed for the dynamic dispatch of Send()
            var result = await _mediator.Send(request.Request).ConfigureAwait(false);
            return new MediatorResponse(result);
        }
        catch (InvalidOperationException ex) when (ex.Source == "MediatR" && ex.InnerException != null && ex.InnerException.Message.Contains("No service for type 'MediatR.IRequestHandler`2["))
        {
            throw new InvalidOperationException("DurableMediator should be included in the <InternalsVisibleTo> so it can reference the internal classes", ex);
        }
    }
}
