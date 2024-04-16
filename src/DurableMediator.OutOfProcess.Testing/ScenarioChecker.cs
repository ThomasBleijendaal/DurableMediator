using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DurableMediator.OutOfProcess.Testing;

internal class ScenarioChecker<TWorkflowRequest> : IWorkflowExecution<TWorkflowRequest>
{
    private readonly ScenarioRun _scenarioRun;
    private readonly IWorkflowExecution<TWorkflowRequest> _execution;
    private readonly IEnumerator<object> _scenarioEnumerator;

    public ScenarioChecker(
        ScenarioRun scenarioRun,
        IWorkflowExecution<TWorkflowRequest> execution,
        IEnumerator<object> scenarioEnumerator)
    {
        _scenarioRun = scenarioRun;
        _execution = execution;
        _scenarioEnumerator = scenarioEnumerator;

        OrchestrationContext = new TaskOrchestrationContextChecker(this, execution.OrchestrationContext, _scenarioRun.CurrentUtcDateTime);
    }

    public TaskOrchestrationContext OrchestrationContext { get; }

    public ILogger ReplaySafeLogger => _execution.ReplaySafeLogger;

    public TWorkflowRequest Request => _execution.Request;

    public int Invocations { get; set; }

    public Task CallSubWorkflowAsync(IWorkflowRequest request)
    {
        CheckRequest(request);
        return _execution.CallSubWorkflowAsync(request);
    }

    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
    {
        CheckRequest(request);
        return _execution.CallSubWorkflowAsync(request);
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken token)
    {
        CheckRequest(new Scenario.CreateDelay(delay));
        return _execution.DelayAsync(delay, token);
    }

    public Task SendAsync(IRequest request)
    {
        CheckRequest(request);
        return _execution.SendAsync(request);
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        CheckRequest(request);
        return _execution.SendAsync(request);
    }

    public Task<TResponse> SendWithCheckAsync<TResponse>(IRequest<TResponse> request, IRequest<TResponse?> checkIfRequestApplied, int maxAttempts = 3, TimeSpan? delay = null)
    {
        CheckRequest(new Scenario.CheckRequest(request, checkIfRequestApplied, maxAttempts, delay));
        return _execution.SendWithCheckAsync(request, checkIfRequestApplied, maxAttempts, delay);
    }

    public Task SendWithDelayAsync(IRequest request, TimeSpan? delay, CancellationToken token)
    {
        CheckRequest(request);
        return _execution.SendWithDelayAsync(request, delay, token);
    }

    public Task<TResponse> SendWithDelayAsync<TResponse>(IRequest<TResponse> request, TimeSpan? delay, CancellationToken token)
    {
        CheckRequest(request);
        return _execution.SendWithDelayAsync(request, delay, token);
    }

    public Task SendWithRetryAsync(IRequest request, int maxAttempts = 3, TimeSpan? delay = null)
    {
        CheckRequest(request);
        return _execution.SendWithRetryAsync(request, maxAttempts, delay);
    }

    public Task<TResponse> SendWithRetryAsync<TResponse>(IRequest<TResponse> request, int maxAttempts = 3, TimeSpan? delay = null)
    {
        CheckRequest(request);
        return _execution.SendWithRetryAsync(request, maxAttempts, delay);
    }

    internal void CheckRequest<TRequest>(TRequest request)
    {
        Invocations++;

        if (!_scenarioEnumerator.MoveNext())
        {
            Assert.Fail($"Scenario failed to produce next request at invocation {Invocations}.");
        }

        var expectedRequest = _scenarioEnumerator.Current;

        Assert.That(expectedRequest, Is.EqualTo(request), $"Incorrect request at invocation {Invocations}.");

        _scenarioRun.AddTick();
        ((TaskOrchestrationContextChecker)OrchestrationContext).AddTick();
    }

    private sealed class TaskOrchestrationContextChecker : TaskOrchestrationContext
    {
        private readonly ScenarioChecker<TWorkflowRequest> _scenarioChecker;
        private readonly TaskOrchestrationContext _context;
        private readonly GuidGenerator _guidGenerator = new();

        private DateTime _currentUtcDateTime;

        public TaskOrchestrationContextChecker(
            ScenarioChecker<TWorkflowRequest> scenarioChecker,
            TaskOrchestrationContext context,
            DateTime dateTime)
        {
            _scenarioChecker = scenarioChecker;
            _context = context;
            _currentUtcDateTime = dateTime;
        }

        public void AddTick()
        {
            _currentUtcDateTime += TimeSpan.FromSeconds(1);
        }

        public override TaskName Name => _context.Name;

        public override string InstanceId => _context.InstanceId;

        public override ParentOrchestrationInstance? Parent => _context.Parent;

        public override DateTime CurrentUtcDateTime => _currentUtcDateTime;

        public override bool IsReplaying => _context.IsReplaying;

        protected override ILoggerFactory LoggerFactory => Mock.Of<ILoggerFactory>();

        public override Task<TResult> CallActivityAsync<TResult>(TaskName name, object? input = null, TaskOptions? options = null)
        {
            return _context.CallActivityAsync<TResult>(name, input, options);
        }

        public override Task<TResult> CallSubOrchestratorAsync<TResult>(TaskName orchestratorName, object? input = null, TaskOptions? options = null)
        {
            return _context.CallSubOrchestratorAsync<TResult>(orchestratorName, input, options);
        }

        public override void ContinueAsNew(object? newInput = null, bool preserveUnprocessedEvents = true)
        {
            _context.ContinueAsNew(newInput, preserveUnprocessedEvents);
        }

        public override Task CreateTimer(DateTime fireAt, CancellationToken cancellationToken)
        {
            _scenarioChecker.CheckRequest(new Scenario.CreateTimer(fireAt));
            return _context.CreateTimer(fireAt, cancellationToken);
        }

        public override Task CreateTimer(TimeSpan delay, CancellationToken cancellationToken)
        {
            _scenarioChecker.CheckRequest(new Scenario.CreateDelay(delay));
            return _context.CreateTimer(delay, cancellationToken);
        }

        public override T GetInput<T>()
        {
            return _context.GetInput<T>()!;
        }

        public override Guid NewGuid() => _guidGenerator.GetGuid();

        public override void SendEvent(string instanceId, string eventName, object payload)
        {
            _context.SendEvent(instanceId, eventName, payload);
        }

        public override void SetCustomStatus(object? customStatus)
        {
            _context.SetCustomStatus(customStatus);
        }

        public override Task<T> WaitForExternalEvent<T>(string eventName, CancellationToken cancellationToken = default)
        {
            return _context.WaitForExternalEvent<T>(eventName, cancellationToken);
        }
    }
}
