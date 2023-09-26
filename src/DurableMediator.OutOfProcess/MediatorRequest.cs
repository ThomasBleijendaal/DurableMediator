using System.Text.Json.Serialization;
using MediatR;

namespace DurableMediator.OutOfProcess;

// TODO: is this one correct?
[JsonConverter(typeof(MediatorRequestWithResponseJsonConverter))]
public record MediatorRequest(IRequest<Unit> Request);
