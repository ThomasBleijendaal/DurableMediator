using System.Text.Json.Serialization;

namespace DurableMediator.OutOfProcess;

[JsonConverter(typeof(MediatorRequestJsonConverter))]
public record MediatorRequestWithResponse(dynamic Request);
