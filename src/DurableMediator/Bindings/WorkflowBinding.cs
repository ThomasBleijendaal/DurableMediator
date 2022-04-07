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
        BindToInput<WorkflowMonitor>(rule);
        BindToInput<WorkflowOrchestrator>(rule);
    }

    private static void BindToInput<TService>(FluentBindingRule<WorkflowAttribute> rule)
        => rule.BindToInput((attr, context) =>
        {
            var service = context.FunctionContext.CreateObjectInstance<TService>();

            return Task.FromResult(service);
        });
}
