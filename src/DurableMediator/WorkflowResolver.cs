﻿using Microsoft.Extensions.DependencyInjection;

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

        var workflowType = typeof(IWorkflow<,>).MakeGenericType(descriptor.Request, descriptor.Response);
        var wrapperType = typeof(WorkflowWrapper<,>).MakeGenericType(descriptor.Request, descriptor.Response);

        var wrapper = Activator.CreateInstance(
            wrapperType,
            _serviceProvider.GetRequiredService(workflowType),
            _serviceProvider.GetRequiredService<ITracingProvider>())
            ?? throw new InvalidOperationException("Failed to create workflow wrapper");

        return (IWorkflowWrapper)wrapper;
    }
}
