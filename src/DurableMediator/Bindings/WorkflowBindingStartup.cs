using DurableMediator.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(WorkflowBindingStartup))]

namespace DurableMediator.Bindings;

internal class WorkflowBindingStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddWorkflowBinding();
    }
}
