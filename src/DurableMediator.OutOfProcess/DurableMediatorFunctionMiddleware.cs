using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

internal class DurableMediatorFunctionMiddleware : IDurableMediatorMiddleware
{
    private readonly ILogger<DurableMediatorFunction> _logger;

    private IDisposable? _logScope;

    public DurableMediatorFunctionMiddleware(
        ILogger<DurableMediatorFunction> logger)
    {
        _logger = logger;
    }

    public Task PreProcessAsync<TRequest>(TRequest request, string instanceId)
    {
        _logScope = _logger.BeginScope(new Dictionary<string, object?> { { "instanceId", instanceId } });

        return Task.CompletedTask;
    }
    public Task PostProcessAsync<TRequest, TResponse>(TRequest request, TResponse response, string instanceId)
    {
        _logScope?.Dispose();

        return Task.CompletedTask;
    }
}
