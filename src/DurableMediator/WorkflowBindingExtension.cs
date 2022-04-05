using Microsoft.Azure.WebJobs;


namespace DurableMediator;

internal static class WorkflowBindingExtension
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
