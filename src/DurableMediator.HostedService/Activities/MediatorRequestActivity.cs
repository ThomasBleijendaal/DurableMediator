using System.Runtime.ExceptionServices;
using DurableMediator.HostedService.Models;
using DurableTask.Core;
using MediatR;

namespace DurableMediator.HostedService.Activities;

internal class MediatorRequestActivity : AsyncTaskActivity<MediatorRequest, Unit>, IActivity
{
    public static string Name => "Request";

    private readonly IMediator _mediator;
    private readonly IDurableMediatorMiddleware[] _middlewares;

    public MediatorRequestActivity(
        IMediator mediator,
        IEnumerable<IDurableMediatorMiddleware> middlewares)
    {
        _mediator = mediator;
        _middlewares = middlewares.ToArray();
    }

    protected override async Task<Unit> ExecuteAsync(TaskContext context, MediatorRequest input)
    {
        for (var i = 0; i < _middlewares.Length; i++)
        {
            await _middlewares[i].PreProcessAsync(input.Request, context.OrchestrationInstance.InstanceId);
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

        for (var i = _middlewares.Length - 1; i >= 0; i--)
        {
            await _middlewares[i].PostProcessAsync(input.Request, Unit.Value, context.OrchestrationInstance.InstanceId);
        }

        if (exception != null)
        {
            ExceptionDispatchInfo.Throw(exception);
        }

        return Unit.Value;
    }
}
