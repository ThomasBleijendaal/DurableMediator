using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json;

namespace DurableMediator.Metadata;

internal class WorkflowMetadata
{
    public WorkflowMetadata(string name)
    {
        Name = name;
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = "workflow";

    [JsonProperty("direction")]
    public BindingDirection Direction { get; set; } = BindingDirection.In;
}
