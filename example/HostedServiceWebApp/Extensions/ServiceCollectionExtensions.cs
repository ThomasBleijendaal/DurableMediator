using DurableTask.AzureStorage;

namespace HostedServiceWebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorageOrchestrationService(
        this IServiceCollection services,
        bool useAppLease,
        Action<AzureStorageOrchestrationServiceSettings>? configure = null)
    {
        services.AddTransient(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var storageConnectionString = config.GetValue<string>("AzureStorageConnectionString")
               ?? throw new InvalidOperationException();

            var taskHubName = config.GetValue<string>("TaskHubName")
                ?? throw new InvalidOperationException();

            var azureStorageSettings = new AzureStorageOrchestrationServiceSettings
            {
                StorageAccountClientProvider = new StorageAccountClientProvider(storageConnectionString),
                TaskHubName = taskHubName,
                UseAppLease = useAppLease
            };

            configure?.Invoke(azureStorageSettings);

            return new AzureStorageOrchestrationService(azureStorageSettings);
        });

        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining(typeof(ServiceCollectionExtensions)));

        return services;
    }
}
