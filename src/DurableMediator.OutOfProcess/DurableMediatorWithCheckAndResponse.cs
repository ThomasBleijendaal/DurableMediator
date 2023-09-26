using MediatR;
using Microsoft.DurableTask;

namespace DurableMediator.OutOfProcess;

public class DurableMediatorWithCheckAndResponse : TaskActivity<MediatorRequestWithCheckAndResponse, MediatorResponse>
{
    private readonly IMediator _mediator;

    public DurableMediatorWithCheckAndResponse(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<MediatorResponse> RunAsync(TaskActivityContext context, MediatorRequestWithCheckAndResponse input)
    {
        if (await _mediator.Send(input.CheckIfRequestApplied).ConfigureAwait(false) is { } result)
        {
            return new MediatorResponse(result);
        }

        return new MediatorResponse(await _mediator.Send(input.Request));
    }
}
