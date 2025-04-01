using DurableTask.Core;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService.Plumbing;

internal class WorkflowTaskOrchestration<TWorkflow, TRequest> : TaskOrchestration
    where TWorkflow : IWorkflow<TRequest>, new()
    where TRequest : IWorkflowRequest
{
    private readonly ILogger _logger;

    public WorkflowTaskOrchestration(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task<string> Execute(OrchestrationContext context, string input)
    {
        await Workflow.StartAsync<TWorkflow, TRequest>(context, _logger, input);
        return string.Empty;
    }

    public override string GetStatus()
    {
        return string.Empty;
    }

    public override void RaiseEvent(OrchestrationContext context, string name, string input)
    {

    }
}

internal class WorkflowTaskOrchestration<TWorkflow, TRequest, TResponse> : TaskOrchestration
    where TWorkflow : IWorkflow<TRequest, TResponse>, new()
    where TRequest : IWorkflowRequest
{
    private readonly ILogger _logger;

    public WorkflowTaskOrchestration(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task<string> Execute(OrchestrationContext context, string input)
    {
        var response = await Workflow.StartAsync<TWorkflow, TRequest, TResponse>(context, _logger, input);
        return response;
    }

    public override string GetStatus()
    {
        return string.Empty;
    }

    public override void RaiseEvent(OrchestrationContext context, string name, string input)
    {

    }
}
