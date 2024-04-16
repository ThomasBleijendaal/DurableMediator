using System.Runtime.ExceptionServices;
using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace DurableMediator.OutOfProcess;

public class DurableMediatorFunction
{
    public const string DurableMediatorName = "Request";
    public const string DurableMediatorWithResponseName = "RequestWithResponse";
    public const string DurableMediatorWithCheckAndResponseName = "RequestWithResponseCheck";

    private readonly IMediator _mediator;
    private readonly IReadOnlyList<IDurableMediatorMiddleware> _middlewares;

    public DurableMediatorFunction(
        IMediator mediator,
        IEnumerable<IDurableMediatorMiddleware> middlewares)
    {
        _mediator = mediator;
        _middlewares = middlewares.ToList();
    }

    [Function(DurableMediatorName)]
    public async Task<Unit> DurableMediatorAsync([ActivityTrigger] MediatorRequest input, string instanceId)
    {
        for (var i = 0; i < _middlewares.Count; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, instanceId);
        }

        Exception? exception = null;
        try
        {
            await _mediator.Send(input.Request).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, Unit.Value, instanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        return Unit.Value;
    }

    [Function(DurableMediatorWithResponseName)]
    public async Task<MediatorResponse> DurableMediatorWithResponseAsync([ActivityTrigger] MediatorRequestWithResponse input, string instanceId)
    {
        for (var i = 0; i < _middlewares.Count; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, instanceId);
        }

        Exception? exception = null;
        object? result = null;
        try
        {
            result = await _mediator.Send((object)input.Request).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, result, instanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        return new MediatorResponse(result);
    }

    [Function(DurableMediatorWithCheckAndResponseName)]
    public async Task<MediatorResponse> DurableMediatorWithCheckAndResponseAsync([ActivityTrigger] MediatorRequestWithCheckAndResponse input, string instanceId)
    {
        for (var i = 0; i < _middlewares.Count; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, instanceId);
        }

        Exception? exception = null;
        object? result = null;

        try
        {
            result = await _mediator.Send(input.CheckIfRequestApplied).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, result, instanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        if (result is { } checkResult)
        {
            return new MediatorResponse(checkResult);
        }

        for (var i = 0; i < _middlewares.Count; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, instanceId);
        }

        result = null;
        try
        {
            result = await _mediator.Send((object)input.Request).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, result, instanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        return new MediatorResponse(result);
    }
}
