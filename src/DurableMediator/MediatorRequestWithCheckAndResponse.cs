using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record MediatorRequestWithCheckAndResponse(
    Tracing Tracing,
    string InstanceId,
    dynamic Request,
    dynamic CheckRequest);
