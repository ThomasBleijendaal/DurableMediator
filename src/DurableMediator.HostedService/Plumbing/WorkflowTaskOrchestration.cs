using DurableTask.Core;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService.Plumbing;

internal class WorkflowTaskOrchestration<TWorkflow, TRequest> : TaskOrchestration
    where TWorkflow : IWorkflow<TRequest>, new()
    where TRequest : IWorkflowRequest
{
    private readonly ILogger _logger;
    private WorkflowOrchestrationContext? _context;

    public WorkflowTaskOrchestration(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task<string> Execute(OrchestrationContext context, string input)
    {
        _context ??= new()
        {
            OrchestrationContext = context
        };

        var task = Workflow.StartAsync<TWorkflow, TRequest>(_context, _logger, input);
        await Task.Yield();
        await task;

        return string.Empty;
    }

    public override string GetStatus()
    {
        return string.Empty;
    }

    public override void RaiseEvent(OrchestrationContext context, string name, string input)
    {
        _context?.CompleteTask(name, input);
    }
}

internal class WorkflowTaskOrchestration<TWorkflow, TRequest, TResponse> : TaskOrchestration
    where TWorkflow : IWorkflow<TRequest, TResponse>, new()
    where TRequest : IWorkflowRequest<TResponse>
{
    private readonly ILogger _logger;
    private WorkflowOrchestrationContext? _context;

    public WorkflowTaskOrchestration(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task<string> Execute(OrchestrationContext context, string input)
    {
        _context ??= new()
        {
            OrchestrationContext = context
        };

        var response = await Workflow.StartAsync<TWorkflow, TRequest, TResponse>(_context, _logger, input);
        return response;
    }

    public override string GetStatus()
    {
        return string.Empty;
    }

    public override void RaiseEvent(OrchestrationContext context, string name, string input)
    {
        _context?.CompleteTask(name, input);
    }
}
