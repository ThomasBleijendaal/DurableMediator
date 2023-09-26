using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace DurableMediator.OutOfProcess;

public class DurableMediatorFunction
{
    public const string DurableMediatorName = "Request";
    public const string DurableMediatorWithResponseName = "RequestWithResponse";
    public const string DurableMediatorWithCheckAndResponseName = "RequestWithResponseCheck";
    private readonly IMediator _mediator;

    public DurableMediatorFunction(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Function(DurableMediatorName)]
    public async Task<Unit> DurableMediatorAsync([ActivityTrigger] MediatorRequest input, string instanceId, FunctionContext executionContext)
    {
        return await _mediator.Send(input.Request).ConfigureAwait(false);
    }

    [Function(DurableMediatorWithResponseName)]
    public async Task<MediatorResponse> DurableMediatorWithResponseAsync([ActivityTrigger] MediatorRequestWithResponse input, string instanceId, FunctionContext executionContext)
    {
        return new MediatorResponse(await _mediator.Send(input.Request).ConfigureAwait(false));
    }

    [Function(DurableMediatorWithCheckAndResponseName)]
    public async Task<MediatorResponse> DurableMediatorWithCheckAndResponseAsync([ActivityTrigger] MediatorRequestWithCheckAndResponse input, string instanceId, FunctionContext executionContext)
    {
        if (await _mediator.Send(input.CheckIfRequestApplied).ConfigureAwait(false) is { } result)
        {
            return new MediatorResponse(result);
        }

        return new MediatorResponse(await _mediator.Send(input.Request).ConfigureAwait(false));
    }
}
