using System.Text.Json;
using DurableTask.Core;
using DurableTask.Core.Serializing;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService;

internal abstract class Workflow
{
    public static async Task StartAsync<TWorkflow, TRequest>(OrchestrationContext context, ILogger replayUnsafeLogger, string input)
        where TWorkflow : IWorkflow<TRequest>, new()
    {
        var workflow = new TWorkflow();

        var logger = new ReplaySafeLogger(context, replayUnsafeLogger);

        logger.BeginScope(new Dictionary<string, object?> { { "instanceId", context.OrchestrationInstance.InstanceId } });

        var request = JsonSerializer.Deserialize<TRequest>(input) ?? throw new InvalidOperationException("Input invalid");

        var executor = new OrchestratorExecutor<TRequest>(context, request, logger);
        await workflow.OrchestrateAsync(executor);
    }

    public static async Task<string> StartAsync<TWorkflow, TRequest, TResponse>(OrchestrationContext context, ILogger replayUnsafeLogger, string input)
        where TWorkflow : IWorkflow<TRequest, TResponse>, new()
    {
        var workflow = new TWorkflow();

        var logger = new ReplaySafeLogger(context, replayUnsafeLogger);

        logger.BeginScope(new Dictionary<string, object?> { { "instanceId", context.OrchestrationInstance.InstanceId } });

        var request = JsonSerializer.Deserialize<TRequest>(input) ?? throw new InvalidOperationException("Input invalid");

        var executor = new OrchestratorExecutor<TRequest>(context, request, logger);

        var response = await workflow.OrchestrateAsync(executor);

        return JsonDataConverter.Default.Serialize(response);
    }

    private sealed class ReplaySafeLogger : ILogger
    {
        private readonly OrchestrationContext _context;
        private readonly ILogger _logger;

        internal ReplaySafeLogger(OrchestrationContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!_context.IsReplaying)
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}
