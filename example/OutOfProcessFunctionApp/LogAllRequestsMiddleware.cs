using DurableMediator.OutOfProcess;
using Microsoft.Extensions.Logging;

namespace OutOfProcessFunctionApp;

// example middleware to intercept all requests between the orchestrator and the mediator
// this can be handy to store all requests in the table storage for extra logging
// the in-process durable mediator used durable entities which did this automatically, but
// via this mechanism its possible to restore that behavior
public class LogAllRequestsMiddleware : IDurableMediatorMiddleware
{
    private readonly ILogger<LogAllRequestsMiddleware> _logger;

    private IDisposable? _logScope;

    public LogAllRequestsMiddleware(
        ILogger<LogAllRequestsMiddleware> logger)
    {
        _logger = logger;
    }

    public Task PreProcessAsync<TRequest>(TRequest request, string instanceId)
    {
        _logScope = _logger.BeginScope(new Dictionary<string, object?> { { "request", request } });

        return Task.CompletedTask;
    }

    public Task PostProcessAsync<TRequest, TResponse>(TRequest request, TResponse response, string instanceId)
    {
        _logger.LogInformation("Received {@response}", response);
        _logScope?.Dispose();

        return Task.CompletedTask;
    }
}
