using DurableMediator.HostedService.Constants;
using DurableTask.Core;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService.Plumbing;

internal class WorkflowCreator<TWorkflow, TRequest> : ObjectCreator<TaskOrchestration>
    where TWorkflow : IWorkflow<TRequest>, new()
    where TRequest : IWorkflowRequest
{
    private readonly ILogger _logger;

    public WorkflowCreator(string name, ILogger logger)
    {
        Name = name;
        Version = Versions.Default;

        _logger = logger;
    }

    public override TaskOrchestration Create() => new WorkflowTaskOrchestration<TWorkflow, TRequest>(_logger);
}

internal class WorkflowCreator<TWorkflow, TRequest, TResponse> : ObjectCreator<TaskOrchestration>
    where TWorkflow : IWorkflow<TRequest, TResponse>, new()
    where TRequest : IWorkflowRequest
{
    private readonly ILogger _logger;

    public WorkflowCreator(string name, ILogger logger)
    {
        Name = name;
        Version = Versions.Default;

        _logger = logger;
    }

    public override TaskOrchestration Create() => new WorkflowTaskOrchestration<TWorkflow, TRequest, TResponse>(_logger);
}
