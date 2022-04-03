using MediatR;
using Newtonsoft.Json;

namespace DurableMediator;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record WorkflowRequest(IRequest<Unit> Request);
