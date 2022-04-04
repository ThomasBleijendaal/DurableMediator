using DurableMediator;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(WorkflowBindingStartup))]

namespace DurableMediator;

internal static class WorkflowOrchestratorFunction
{
    public static Task OrchestarateAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext ctx,
        [Workflow] IWorkflowOrchestrator orchestrator)
        => orchestrator.OrchestrateAsync(ctx);
}


[Binding]
[AttributeUsage(AttributeTargets.Parameter)]
public class WorkflowAttribute : Attribute
{
}

[Extension(nameof(WorkflowBinding))]
public class WorkflowBinding : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var rule = context.AddBindingRule<WorkflowAttribute>();
        rule.BindToInput((WorkflowAttribute attr, ValueBindingContext context) =>
        {
            var x = context.FunctionContext.CreateObjectInstance<WorkflowOrchestrator>();

            return Task.FromResult(x);
        });
    }
}

public static class WorkflowBindingExtension
{
    public static IWebJobsBuilder AddWorkflowBinding(this IWebJobsBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddExtension<WorkflowBinding>();
        return builder;
    }
}

public class WorkflowBindingStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddWorkflowBinding();
    }
}
