using Newtonsoft.Json;

namespace DurableMediator;

internal record GenericWorkflowRequest(
    string InstanceId,
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)] object Request) : IWorkflowRequest;
