﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator;

internal class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly IWorkflowResolver _workflowResolver;
    private readonly ILoggerFactory _loggerFactory;

    public WorkflowOrchestrator(
        IWorkflowResolver workflowResolver,
        ILoggerFactory loggerFactory)
    {
        _workflowResolver = workflowResolver;
        _loggerFactory = loggerFactory;
    }

    public async Task OrchestrateAsync(IDurableOrchestrationContext context)
    {
        var workflowWrapper = _workflowResolver.GetWorkflow(context.Name);

        var entityId = _workflowResolver.GetEntityId(context);

        var proxy = context.CreateEntityProxy<IDurableMediator>(entityId);

        var logger = _loggerFactory.CreateLogger(workflowWrapper.WorkflowType);

        var replaySafeLogger = context.CreateReplaySafeLogger(logger);

        await workflowWrapper.OrchestrateAsync(context, entityId, proxy, replaySafeLogger).ConfigureAwait(true);
    }
}
