using MediatR;
using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record MediatorRequest(IRequest<Unit> Request);
