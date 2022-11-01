using Newtonsoft.Json;

namespace DurableMediator.Metadata;

internal class ActivityTriggerMetadata
{
    [JsonProperty("name")]
    public string Name { get; set; } = "request";

    [JsonProperty("type")]
    public string Type { get; set; } = "activityTrigger";
}
