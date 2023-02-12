using System.Text.Json.Serialization;

namespace DurableMediator.OutOfProcess;

[JsonConverter(typeof(MediatorRequestWithResponseJsonConverter))]
public record MediatorRequestWithResponse(dynamic Request);
