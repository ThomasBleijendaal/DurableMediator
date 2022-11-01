using MediatR;
using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record MediatorRequest(
    Tracing Tracing,
    string InstanceId,
    IRequest<Unit> Request);
