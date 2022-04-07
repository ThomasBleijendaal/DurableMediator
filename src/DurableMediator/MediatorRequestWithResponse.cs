using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record MediatorRequestWithResponse(dynamic Request);
