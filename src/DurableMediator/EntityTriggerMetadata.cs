using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json;

namespace DurableMediator;

public class EntityTriggerMetadata
{
    [JsonProperty("name")]
    public string Name { get; set; } = "ctx";

    [JsonProperty("type")]
    public string Type { get; set; } = "entityTrigger";

    [JsonProperty("direction")]
    public BindingDirection Direction { get; set; } = BindingDirection.In;
}
