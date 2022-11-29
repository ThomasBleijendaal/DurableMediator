using System.ComponentModel.DataAnnotations;

namespace DurableMediator;

internal class WorkflowConfiguration
{
    public static bool UseExperimentalEntityExecution;

    [Required(ErrorMessage = "Make sure durableTask.hubName is set in host.json")]
    public string HubName { get; set; } = null!;
}
