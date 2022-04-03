using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Script.Description;
using Microsoft.Extensions.DependencyInjection;

namespace DurableMediator;

public static class FunctionsHostBuilderExtensions
{
    public static void AddDurableMediator(this IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;

        builder.Services.AddOptions<WorkflowConfiguration>()
            .Bind(config.GetSection("AzureFunctionsJobHost:extensions:durableTask"))
            .ValidateDataAnnotations();

        builder.Services.AddTransient<IWorkflowMonitor, WorkflowMonitor>();
        builder.Services.AddTransient(typeof(IWorkflowOrchestrator<,>), typeof(WorkflowOrchestrator<,>));

        builder.Services.AddSingleton<IFunctionProvider, DurableMediatorFunctionProvider>();
    }
}
