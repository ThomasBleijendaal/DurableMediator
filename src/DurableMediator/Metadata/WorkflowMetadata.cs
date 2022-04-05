using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json;

namespace DurableMediator.Metadata;

internal class WorkflowMetadata
{
    [JsonProperty("name")]
    public string Name { get; set; } = "orchestrator";

    [JsonProperty("type")]
    public string Type { get; set; } = "workflow";

    [JsonProperty("direction")]
    public BindingDirection Direction { get; set; } = BindingDirection.In;
}
