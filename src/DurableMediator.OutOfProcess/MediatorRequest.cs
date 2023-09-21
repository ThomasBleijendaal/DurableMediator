using System.Text.Json.Serialization;
using MediatR;

namespace DurableMediator.OutOfProcess;

[JsonConverter(typeof(MediatorRequestJsonConverter))]
public record MediatorRequest(IRequest<Unit> Request)
{
    public string RequestType => Request.GetType().AssemblyQualifiedName ?? Request.GetType().Name;
}
