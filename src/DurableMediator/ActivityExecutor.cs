using MediatR;
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
            request.InstanceId,
            request.Request.GetType().Name);

        try
        {
            await _mediator.Send(request.Request).ConfigureAwait(false);
        }
        catch (Exception ex) when (_logger.LogException(ex, "Activity failed - failed to run request"))
        {
            throw;
        }
    }

    public async Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request)
    {
        using var _ = _logger.BeginTracingScope(
            _tracingProvider,
            request.Tracing,
            request.InstanceId,
            (string)request.Request.GetType().Name);

        try
        {
            // the dynamic is needed for the dynamic dispatch of Send()
            return new MediatorResponse(await _mediator.Send(request.Request).ConfigureAwait(false));
        }
        catch (InvalidOperationException ex) when (InternalsVisibleToIssue(ex))
        {
            throw InternalsVisibleToException(ex);
        }
        catch (Exception ex) when (_logger.LogException(ex, "Activity failed - failed to run request"))
        {
            throw;
        }
    }

    public async Task<MediatorResponse> SendObjectWithCheckAndResponseAsync(MediatorRequestWithCheckAndResponse request)
    {
        using var _ = _logger.BeginTracingScope(
            _tracingProvider,
            request.Tracing,
            request.InstanceId,
            (string)request.Request.GetType().Name);

        try
        {
            // the dynamic is needed for the dynamic dispatch of Send()
            if (await _mediator.Send(request.CheckRequest).ConfigureAwait(false) is { } result && result is not null)
            {
                return new MediatorResponse(result);
            }
        }
        catch (InvalidOperationException ex) when (InternalsVisibleToIssue(ex))
        {
            throw InternalsVisibleToException(ex);
        }
        catch (Exception ex) when (_logger.LogException(ex, "Activity failed - failed to run check"))
        {
            throw;
        }

        try
        {
            // the dynamic is needed for the dynamic dispatch of Send()
            return new MediatorResponse(await _mediator.Send(request.Request).ConfigureAwait(false));
        }
        catch (InvalidOperationException ex) when (InternalsVisibleToIssue(ex))
        {
            throw InternalsVisibleToException(ex);
        }
        catch (Exception ex) when (_logger.LogException(ex, "Activity failed - failed to run request"))
        {
            throw;
        }
    }

    private static bool InternalsVisibleToIssue(InvalidOperationException ex)
        => ex.Source == "MediatR" &&
           ex.InnerException != null &&
           ex.InnerException.Message.Contains("No service for type 'MediatR.IRequestHandler`2[");

    private static InvalidOperationException InternalsVisibleToException(InvalidOperationException ex)
        => new InvalidOperationException("DurableMediator should be included in the <InternalsVisibleTo> so it can reference the internal classes", ex);
}
