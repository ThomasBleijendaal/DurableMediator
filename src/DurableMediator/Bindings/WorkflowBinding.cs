using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace DurableMediator.Bindings;

[Extension(nameof(WorkflowBinding))]
internal class WorkflowBinding : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var rule = context.AddBindingRule<WorkflowAttribute>();
        BindToInput<WorkflowStarter>(rule);
        BindToInput<WorkflowManagement>(rule);
        BindToInput<WorkflowOrchestrator>(rule);
        BindToInput<ActivityExecutor>(rule);
    }

#pragma warning disable CS0618 // Type or member is obsolete - going to consume it anyway
    private static void BindToInput<TService>(FluentBindingRule<WorkflowAttribute> rule)
#pragma warning restore CS0618 // Type or member is obsolete - going to consume it anyway
        => rule.BindToInput((attr, context) =>
        {
            var service = context.FunctionContext.CreateObjectInstance<TService>();

            return Task.FromResult(service);
        });
}
