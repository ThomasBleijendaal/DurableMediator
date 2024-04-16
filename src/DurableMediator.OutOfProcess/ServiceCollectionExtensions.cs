using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;

namespace DurableMediator.OutOfProcess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDurableMediator(this IServiceCollection services)
    {
        services.AddTransient<IDurableMediatorMiddleware, DurableMediatorFunctionMiddleware>();

        return services;
    }

    public static IServiceCollection AddWorkflowStarter(this IServiceCollection services)
    {
        services.AddTransient<IWorkflowStarter, WorkflowStarter>();
        services.AddDurableTaskClient(nameof(DurableMediator), builder =>
        {
            builder.UseGrpc().RegisterDirectly();
        });

        return services;
    }

    public static IServiceCollection AddSyncWorkflowClient(this IServiceCollection services)
    {
        services.AddTransient<ISyncWorkflowClient, SyncWorkflowClient>();

        return services;
    }
}
