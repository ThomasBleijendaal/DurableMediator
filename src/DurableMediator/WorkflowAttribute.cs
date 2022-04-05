using Microsoft.Azure.WebJobs.Description;

namespace DurableMediator;

[Binding]
[AttributeUsage(AttributeTargets.Parameter)]
public class WorkflowAttribute : Attribute
{
}
