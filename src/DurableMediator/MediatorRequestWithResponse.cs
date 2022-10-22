using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record MediatorRequestWithResponse(
    Tracing Tracing,
    string InstanceId,
    dynamic Request);
