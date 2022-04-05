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
        catch (InvalidOperationException ex)
        {
            // TODO: fix this
            throw new Exception("The common project should be included in the <InternalsVisibleTo> so it can reference the internal classes", ex);
        }
    }
}
