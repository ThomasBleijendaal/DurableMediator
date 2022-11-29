using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DurableMediator;

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

    public IWorkflowWrapper GetWorkflow(string workflowRequestName)
    {
        if (!_workflowDescriptors.TryGetValue(workflowRequestName, out var descriptor))
        {
            throw new InvalidOperationException($"Cannot find workflow associated with {workflowRequestName}");
        }

        var config = _serviceProvider.GetRequiredService<IOptions<WorkflowConfiguration>>().Value;

        var workflowType = typeof(IWorkflow<,>).MakeGenericType(descriptor.Request, descriptor.Response);
        var wrapperType = typeof(WorkflowWrapper<,>).MakeGenericType(descriptor.Request, descriptor.Response);

        var wrapper = Activator.CreateInstance(
            wrapperType,
            _serviceProvider.GetRequiredService<IDurableClientFactory>().CreateClient(new DurableClientOptions
            {
                TaskHub = config.HubName
            }),
            _serviceProvider.GetRequiredService(workflowType),
            _serviceProvider.GetRequiredService<ITracingProvider>())
            ?? throw new InvalidOperationException("Failed to create workflow wrapper");

        return (IWorkflowWrapper)wrapper;
    }
}
