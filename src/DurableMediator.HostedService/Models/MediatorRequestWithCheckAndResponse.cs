using System.Text.Json.Serialization;

namespace DurableMediator.HostedService.Models;

[JsonConverter(typeof(MediatorRequestWithCheckAndResponseJsonConverter))]
public record MediatorRequestWithCheckAndResponse(dynamic Request, dynamic CheckIfRequestApplied);
