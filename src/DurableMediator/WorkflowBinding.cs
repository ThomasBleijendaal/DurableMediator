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
        rule.BindToInput((WorkflowAttribute attr, ValueBindingContext context) =>
        {
            var x = context.FunctionContext.CreateObjectInstance<WorkflowOrchestrator>();

            return Task.FromResult(x);
        });
    }
}
