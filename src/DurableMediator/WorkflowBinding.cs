using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;

namespace DurableMediator;

[Extension(nameof(WorkflowBinding))]
internal class WorkflowBinding : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var rule = context.AddBindingRule<WorkflowAttribute>();
        BindToInput<WorkflowOrchestrator>(rule);
        BindToInput<WorkflowMonitor>(rule);
    }

    private static void BindToInput<TService>(FluentBindingRule<WorkflowAttribute> rule) 
        => rule.BindToInput((WorkflowAttribute attr, ValueBindingContext context) =>
        {
            var service = context.FunctionContext.CreateObjectInstance<TService>();

            return Task.FromResult(service);
        });
}
