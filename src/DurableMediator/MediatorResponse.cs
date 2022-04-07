using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record MediatorResponse(object Response);
