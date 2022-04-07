using System.ComponentModel.DataAnnotations;

namespace DurableMediator;

internal class WorkflowConfiguration
{
    [Required(ErrorMessage = "Make sure durableTask.hubName is set in host.json")]
    public string HubName { get; set; } = null!;
}
