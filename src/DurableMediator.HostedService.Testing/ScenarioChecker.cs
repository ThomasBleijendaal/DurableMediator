using DurableTask.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DurableMediator.HostedService.Testing;

internal class ScenarioChecker<TWorkflowRequest> : IWorkflowExecution<TWorkflowRequest>
{
    private readonly ScenarioRun _scenarioRun;
    private readonly IWorkflowExecution<TWorkflowRequest> _execution;
    private readonly GuidGenerator _guidGenerator = new();
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

    public OrchestrationContext OrchestrationContext { get; }

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

    public Guid NewGuid()
    {
        return _guidGenerator.GetGuid();
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

    private sealed class TaskOrchestrationContextChecker : OrchestrationContext
    {
        private readonly ScenarioChecker<TWorkflowRequest> _scenarioChecker;
        private readonly OrchestrationContext _context;

        private DateTime _currentUtcDateTime;

        public TaskOrchestrationContextChecker(
            ScenarioChecker<TWorkflowRequest> scenarioChecker,
            OrchestrationContext context,
            DateTime dateTime)
        {
            _scenarioChecker = scenarioChecker;
            _context = context;
            _currentUtcDateTime = dateTime;
        }

        public override DateTime CurrentUtcDateTime => _currentUtcDateTime;

        public void AddTick()
        {
            _currentUtcDateTime += TimeSpan.FromSeconds(1);
        }

        public override Task<TResult> ScheduleTask<TResult>(string name, string version, params object[] parameters)
        {
            return _context.ScheduleTask<TResult>(name, version, parameters);
        }

        public override Task<T> CreateTimer<T>(DateTime fireAt, T state)
        {
            _scenarioChecker.CheckRequest(new Scenario.CreateTimer(fireAt));
            return _context.CreateTimer(fireAt, state);
        }

        public override Task<T> CreateTimer<T>(DateTime fireAt, T state, CancellationToken cancelToken)
        {
            _scenarioChecker.CheckRequest(new Scenario.CreateTimer(fireAt));
            return _context.CreateTimer(fireAt, state, cancelToken);
        }

        public override Task<T> CreateSubOrchestrationInstance<T>(string name, string version, object input)
        {
            return _context.CreateSubOrchestrationInstance<T>(name, version, input);
        }

        public override Task<T> CreateSubOrchestrationInstance<T>(string name, string version, string instanceId, object input)
        {
            return _context.CreateSubOrchestrationInstance<T>(name, version, instanceId, input);
        }

        public override Task<T> CreateSubOrchestrationInstance<T>(string name, string version, string instanceId, object input, IDictionary<string, string> tags)
        {
            return _context.CreateSubOrchestrationInstance<T>(name, version, instanceId, input);
        }

        public override void SendEvent(OrchestrationInstance orchestrationInstance, string eventName, object eventData)
        {
            _context.SendEvent(orchestrationInstance, eventName, eventData);
        }

        public override void ContinueAsNew(object input)
        {
            _context.ContinueAsNew(input);
        }

        public override void ContinueAsNew(string newVersion, object input)
        {
            _context.ContinueAsNew(newVersion, input);
        }
    }
}
