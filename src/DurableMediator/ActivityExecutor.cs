using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class ActivityExecutor : IActivityExecutor
{
    private readonly IMediator _mediator;
    private readonly ITracingProvider _tracingProvider;
    private readonly ILogger<ActivityExecutor> _logger;

    public ActivityExecutor(
        IMediator mediator,
        ITracingProvider tracingProvider,
        ILogger<ActivityExecutor> logger)
    {
        _mediator = mediator;
        _tracingProvider = tracingProvider;
        _logger = logger;
    }

    public async Task SendObjectAsync(MediatorRequest request)
    {
        using var _ = _logger.BeginTracingScope(
            _tracingProvider,
            request.Tracing,
            new EntityId("fdsa", "fdsa"),
            request.InstanceId,
            request.Request.GetType().Name);

        try
        {
            await _mediator.Send(request.Request).ConfigureAwait(false);
        }
        catch (Exception ex) when (_logger.LogException(ex, "Activity failed"))
        {
            throw;
        }
    }

    public async Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request)
    {
        using var _ = _logger.BeginTracingScope(
            _tracingProvider,
            request.Tracing,
            new EntityId("fdsa", "fdsa"),
            request.InstanceId,
            (string)request.Request.GetType().Name);

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
        catch (Exception ex) when (_logger.LogException(ex, "Activity failed"))
        {
            throw;
        }
    }
}
