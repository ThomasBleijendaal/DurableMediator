using MediatR;

namespace DurableMediator;

public class DurableMediatorEntity : IDurableMediator
{
    private readonly IMediator _mediator;

    public DurableMediatorEntity(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task SendObjectAsync(WorkflowRequest request)
    {
        await _mediator.Send(request.Request);
    }

    public async Task<WorkflowResponse> SendObjectWithResponseAsync(WorkflowRequestWithResponse request)
    {
        try
        {
            // the dynamic is needed for the dynamic dispatch of Send()
            // TODO: ConfigureAwait for all
            var result = await _mediator.Send(request.Request);
            return new WorkflowResponse(result);
        }
        catch (InvalidOperationException ex)
        {
            // TODO: fix this
            throw new Exception("The common project should be included in the <InternalsVisibleTo> so it can reference the internal classes", ex);
        }
    }
}
