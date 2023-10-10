using System.Text.Json.Serialization;

namespace DurableMediator.OutOfProcess;

[JsonConverter(typeof(MediatorRequestWithCheckAndResponseJsonConverter))]
public record MediatorRequestWithCheckAndResponse(dynamic Request, dynamic CheckIfRequestApplied);
