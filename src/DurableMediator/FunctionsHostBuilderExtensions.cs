using System.Reflection;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Script.Description;
using Microsoft.Extensions.DependencyInjection;

namespace DurableMediator;

public static class FunctionsHostBuilderExtensions
{
    public static void AddDurableMediator(
        this IFunctionsHostBuilder builder,
        params Type[] handlerAssemblyMarkerTypes)
    {
        var config = builder.GetContext().Configuration;

        builder.Services.AddOptions<WorkflowConfiguration>()
            .Bind(config.GetSection("AzureFunctionsJobHost:extensions:durableTask"))
            .ValidateDataAnnotations();

        builder.Services.AddTransient<IWorkflowMonitor, WorkflowMonitor>();
        builder.Services.AddTransient<IWorkflowStarter, WorkflowStarter>();
        builder.Services.AddTransient<IWorkflowOrchestrator, WorkflowOrchestrator>();

        builder.Services.AddMediatR(handlerAssemblyMarkerTypes);

        AddWorkflowClasses(builder.Services, handlerAssemblyMarkerTypes.Select(x => x.GetTypeInfo().Assembly));
    }

    // TODO: expand this method to better search like MediatR
    private static void AddWorkflowClasses(IServiceCollection service, IEnumerable<Assembly> assembliesToScan)
    {
        var filteredClasses = assembliesToScan.SelectMany(x => x.DefinedTypes)
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select(type => (workflow: type, workflowInterfaces: type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IWorkflow<,>))))
            .Where(type => type.workflowInterfaces.Any())
            .SelectMany(type => type.workflowInterfaces.Select(@interface =>
            {
                var arguments = @interface.GetGenericArguments();
                return (type.workflow, @interface, request: arguments[0], response: arguments[1]);
            }))
            .ToList();

        var workflows = filteredClasses.ToDictionary(type => type.request.Name, type => new WorkflowDescriptor(type.request, type.response));

        service.AddTransient<IWorkflowResolver>((sp) => new WorkflowResolver(sp, workflows));

        foreach (var (workflow, @interface, request, response) in filteredClasses)
        {
            service.AddTransient(@interface, workflow);
        }

        service.AddSingleton<IFunctionProvider>(new DurableMediatorFunctionProvider(workflows.Keys));
    }
}
