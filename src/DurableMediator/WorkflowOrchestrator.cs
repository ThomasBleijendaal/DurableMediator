using MediatR;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DurableMediator;

internal class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly IWorkflowResolver _workflowResolver;
    private readonly WorkflowConfiguration _config;
    private readonly IDurableClientFactory _durableClientFactory;
    private readonly ILogger<WorkflowOrchestrator> _logger;

    public WorkflowOrchestrator(
        IWorkflowResolver workflowResolver,
        IOptions<WorkflowConfiguration> config,
        IDurableClientFactory durableClientFactory,
        ILogger<WorkflowOrchestrator> logger)
    {
        _workflowResolver = workflowResolver;
        _config = config.Value;
        _durableClientFactory = durableClientFactory;
        _logger = logger;
    }

    public async Task<WorkflowStartResult> StartNewAsync<TRequest, TResponse>(TRequest input)
        where TRequest : IWorkflowRequest<TResponse>
    {
        var client = _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

        var request = new GenericWorkflowRequest(input.InstanceId, input);

        await client.StartNewAsync(typeof(TRequest).Name, request.InstanceId, request);

        return new WorkflowStartResult(request.InstanceId);
    }

    public Task<WorkflowStartResult> StartNewAsync<TRequest>(TRequest input)
        where TRequest : IWorkflowRequest
        => StartNewAsync<TRequest, Unit>(input);

    public async Task OrchestrateAsync(IDurableOrchestrationContext context)
    {
        var workflow = _workflowResolver.GetWorkflow(context.Name);

        var entityId = _workflowResolver.GetEntityId(context);

        using var _ = _logger.BeginScope(new Dictionary<string, object?>
        {
            { "entityId", entityId }
        });

        var proxy = context.CreateEntityProxy<IDurableMediator>(entityId);

        try
        {
            context.SetCustomStatus(new WorkflowState(workflow.Name, null));

            await workflow.OrchestrateAsync(context, entityId, proxy);
        }
        catch (Exception ex) when (LogException(ex, "Orchestration failed"))
        {
            context.SetCustomStatus(new WorkflowState(workflow.Name, ex.Message));

            throw;
        }
    }

    private bool LogException(Exception ex, string message)
    {
        _logger.LogError(ex, message);
        return true;
    }
}

internal interface IWorkflowResolver
{
    IWorkflowWrapper GetWorkflow(string workflowRequestName);

    EntityId GetEntityId(IDurableOrchestrationContext context);
}

internal class WorkflowResolver : IWorkflowResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, WorkflowDescriptor> _workflowDescriptors;

    public WorkflowResolver(
        IServiceProvider serviceProvider,
        Dictionary<string, WorkflowDescriptor> workflowDescriptors)
    {
        _serviceProvider = serviceProvider;
        _workflowDescriptors = workflowDescriptors;
    }

    public EntityId GetEntityId(IDurableOrchestrationContext context)
    {
        var input = context.GetInput<GenericWorkflowRequest>();

        return new EntityId(nameof(DurableMediatorEntity), input.InstanceId);
    }

    public IWorkflowWrapper GetWorkflow(string workflowRequestName)
    {
        if (!_workflowDescriptors.TryGetValue(workflowRequestName, out var descriptor))
        {
            throw new InvalidOperationException($"Cannot find workflow associated with {workflowRequestName}");
        }

        var workflowType = typeof(IWorkflow<,>).MakeGenericType(descriptor.Request, descriptor.Response);
        var wrapperType = typeof(WorkflowWrapper<,>).MakeGenericType(descriptor.Request, descriptor.Response);

        var wrapper = Activator.CreateInstance(wrapperType, _serviceProvider.GetService(workflowType))
            ?? throw new InvalidOperationException("Failed to create workflow wrapper");

        return (IWorkflowWrapper)wrapper;
    }
}

internal record WorkflowDescriptor(Type Request, Type Response);
