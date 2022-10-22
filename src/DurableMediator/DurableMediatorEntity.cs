using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class DurableMediatorEntity : IDurableMediator
{
    private readonly IMediator _mediator;
    private readonly ITracingProvider _tracingProvider;
    private readonly ILogger<DurableMediatorEntity> _logger;

    public DurableMediatorEntity(
        IMediator mediator,
        ITracingProvider tracingProvider,
        ILogger<DurableMediatorEntity> logger)
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
            Entity.Current.EntityId,
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
            Entity.Current.EntityId,
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
