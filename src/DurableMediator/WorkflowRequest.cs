using MediatR;
using Newtonsoft.Json;

namespace DurableMediator;

// TODO: rename 

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public record WorkflowRequest(IRequest<Unit> Request);
