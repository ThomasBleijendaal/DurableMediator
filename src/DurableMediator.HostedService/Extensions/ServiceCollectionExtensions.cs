using DurableMediator.HostedService.Activities;
using DurableMediator.HostedService.HostedServices;
using DurableMediator.HostedService.Plumbing;
using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DurableMediator.HostedService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDurableMediator(
        this IServiceCollection services,
        Func<IServiceProvider, IOrchestrationService> serviceResolver)
    {
        AddTaskHubClient(services);

        services.AddTransient<MediatorRequestActivity>();
        services.AddTransient<MediatorRequestWithResponseActivity>();
        services.AddTransient<MediatorRequestWithCheckAndResponseActivity>();
        services.AddTransient<ObjectCreator<TaskActivity>, ActivityCreator<MediatorRequestActivity>>();
        services.AddTransient<ObjectCreator<TaskActivity>, ActivityCreator<MediatorRequestWithResponseActivity>>();
        services.AddTransient<ObjectCreator<TaskActivity>, ActivityCreator<MediatorRequestWithCheckAndResponseActivity>>();

        services.AddSingleton(serviceResolver);
        services.AddHostedService<DurableBackgroundService>();

        return services;
    }

    public static IServiceCollection AddWorkflowService(
        this IServiceCollection services,
        Func<IServiceProvider, IOrchestrationServiceClient> serviceClientResolver)
    {
        AddTaskHubClient(services);

        services.AddTransient(serviceClientResolver);
        services.AddTransient<IWorkflowService, WorkflowService>();

        return services;
    }

    public static IServiceCollection AddWorkflow<TWorkflow>(this IServiceCollection services)
        where TWorkflow : IWorkflow, new()
    {
        if (typeof(TWorkflow).GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IWorkflow<>)) is { } workflowType)
        {
            var request = workflowType.GenericTypeArguments[0];
            var loggerType = typeof(ILogger<>).MakeGenericType(typeof(TWorkflow));
            var creatorType = typeof(WorkflowCreator<,>).MakeGenericType(typeof(TWorkflow), request);

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService(loggerType);

                return (ObjectCreator<TaskOrchestration>)Activator.CreateInstance(creatorType, [typeof(TWorkflow).Name, logger])!;
            });
        }
        else if (typeof(TWorkflow).GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IWorkflow<,>)) is { } workflowWithResponseType)
        {
            var request = workflowWithResponseType.GenericTypeArguments[0];
            var response = workflowWithResponseType.GenericTypeArguments[1];
            var loggerType = typeof(ILogger<>).MakeGenericType(typeof(TWorkflow));
            var creatorType = typeof(WorkflowCreator<,,>).MakeGenericType(typeof(TWorkflow), request, response);

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService(loggerType);

                return (ObjectCreator<TaskOrchestration>)Activator.CreateInstance(creatorType, [typeof(TWorkflow).Name, logger])!;
            });
        }

        return services;
    }

    private static void AddTaskHubClient(IServiceCollection services)
    {
        if (!services.Any(x => x.ServiceType == typeof(TaskHubClient)))
        {
            services.AddSingleton(sp =>
            {
                var service = sp.GetRequiredService<IOrchestrationServiceClient>();
                return new TaskHubClient(service);
            });
        }
    }
}
