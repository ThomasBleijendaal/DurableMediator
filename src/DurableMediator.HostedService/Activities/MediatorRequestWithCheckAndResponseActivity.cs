using System.Runtime.ExceptionServices;
using DurableMediator.HostedService.Models;
using DurableTask.Core;
using MediatR;

namespace DurableMediator.HostedService.Activities;

internal class MediatorRequestWithCheckAndResponseActivity : AsyncTaskActivity<MediatorRequestWithCheckAndResponse, MediatorResponse>, IActivity
{
    public static string Name => "RequestWithResponseCheck";

    private readonly IMediator _mediator;
    private readonly IDurableMediatorMiddleware[] _middlewares;

    public MediatorRequestWithCheckAndResponseActivity(
        IMediator mediator,
        IEnumerable<IDurableMediatorMiddleware> middlewares)
    {
        _mediator = mediator;
        _middlewares = middlewares.ToArray();
    }

    protected override async Task<MediatorResponse> ExecuteAsync(TaskContext context, MediatorRequestWithCheckAndResponse input)
    {
        for (var i = 0; i < _middlewares.Length; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, context.OrchestrationInstance.InstanceId);
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

        for (var i = _middlewares.Length - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, result, context.OrchestrationInstance.InstanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        if (result is { } checkResult)
        {
            return new MediatorResponse(checkResult);
        }

        for (var i = 0; i < _middlewares.Length; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, context.OrchestrationInstance.InstanceId);
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

        for (var i = _middlewares.Length - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, result, context.OrchestrationInstance.InstanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        return new MediatorResponse(result);
    }
}
