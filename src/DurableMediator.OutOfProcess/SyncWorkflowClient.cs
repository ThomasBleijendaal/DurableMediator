using MediatR;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

internal class SyncWorkflowClient : ISyncWorkflowClient
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<IDurableMediatorMiddleware> _middlewares;
    private readonly ILoggerFactory _loggerFactory;

    public SyncWorkflowClient(
        IMediator mediator,
        IEnumerable<IDurableMediatorMiddleware> middlewares,
        ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _middlewares = middlewares;
        _loggerFactory = loggerFactory;
    }

    public Task RunWorkflowAsync(IWorkflowRequest request)
    {
        var requestType = request.GetType();

        var workflow = request.GetWorkflow();
        var executorType = typeof(OrchestratorExecutor<>).MakeGenericType(requestType);

        var executor = Activator.CreateInstance(executorType,
            new SyncTaskOrchestrationContext(_mediator, _loggerFactory, _middlewares, request, this),
            request,
            _loggerFactory.CreateLogger(workflow.GetType()));

        var workflowType = typeof(IWorkflow<>).MakeGenericType(requestType);
        var orchestrateMethod = workflowType.GetMethod(nameof(IWorkflow<object>.OrchestrateAsync))
            ?? throw new InvalidOperationException($"Cannot find type of workflow {request.WorkflowName}");

        return (Task)orchestrateMethod.Invoke(workflow, [executor])!;
    }

    public Task<TWorkflowResponse> RunWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
    {
        var requestType = request.GetType();

        var workflow = request.GetWorkflow();
        var executorType = typeof(OrchestratorExecutor<>).MakeGenericType(requestType);

        var executor = Activator.CreateInstance(executorType,
            new SyncTaskOrchestrationContext(_mediator, _loggerFactory, _middlewares, request, this),
            request,
            _loggerFactory.CreateLogger(workflow.GetType()));

        var workflowType = typeof(IWorkflow<,>).MakeGenericType(requestType, typeof(TWorkflowResponse));
        var orchestrateMethod = workflowType.GetMethod(nameof(IWorkflow<object, TWorkflowResponse>.OrchestrateAsync))
            ?? throw new InvalidOperationException($"Cannot find type of workflow {request.WorkflowName}");

        return (Task<TWorkflowResponse>)orchestrateMethod.Invoke(workflow, [executor])!;
    }
}
