using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

public class DurableMediatorFunction
{
    public const string DurableMediatorName = "Request";
    public const string DurableMediatorWithResponseName = "RequestWithResponse";
    public const string DurableMediatorWithCheckAndResponseName = "RequestWithResponseCheck";

    private readonly IMediator _mediator;
    private readonly ILogger<DurableMediatorFunction> _logger;

    public DurableMediatorFunction(
        IMediator mediator,
        ILogger<DurableMediatorFunction> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Function(DurableMediatorName)]
    public async Task<Unit> DurableMediatorAsync([ActivityTrigger] MediatorRequest input, string instanceId)
    {
        _logger.BeginScope(new Dictionary<string, object?> { { "instanceId", instanceId } });
        await _mediator.Send(input.Request).ConfigureAwait(false);
        return Unit.Value;
    }

    [Function(DurableMediatorWithResponseName)]
    public async Task<MediatorResponse> DurableMediatorWithResponseAsync([ActivityTrigger] MediatorRequestWithResponse input, string instanceId)
    {
        _logger.BeginScope(new Dictionary<string, object?> { { "instanceId", instanceId } });
        return new MediatorResponse(await _mediator.Send((object)input.Request).ConfigureAwait(false));
    }

    [Function(DurableMediatorWithCheckAndResponseName)]
    public async Task<MediatorResponse> DurableMediatorWithCheckAndResponseAsync([ActivityTrigger] MediatorRequestWithCheckAndResponse input, string instanceId)
    {
        _logger.BeginScope(new Dictionary<string, object?> { { "instanceId", instanceId } });
        if (await _mediator.Send(input.CheckIfRequestApplied).ConfigureAwait(false) is { } result)
        {
            return new MediatorResponse(result);
        }

        return new MediatorResponse(await _mediator.Send((object)input.Request).ConfigureAwait(false));
    }
}
