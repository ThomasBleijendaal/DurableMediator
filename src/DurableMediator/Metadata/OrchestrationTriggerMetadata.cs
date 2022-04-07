using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json;

namespace DurableMediator.Metadata;

internal class OrchestrationTriggerMetadata
{
    [JsonProperty("name")]
    public string Name { get; set; } = "ctx";

    [JsonProperty("type")]
    public string Type { get; set; } = "orchestrationTrigger";

    [JsonProperty("direction")]
    public BindingDirection Direction { get; set; } = BindingDirection.In;
}
