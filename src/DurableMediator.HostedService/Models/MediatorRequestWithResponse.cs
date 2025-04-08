using System.Text.Json.Serialization;

namespace DurableMediator.HostedService.Models;

[JsonConverter(typeof(MediatorRequestWithResponseJsonConverter))]
public record MediatorRequestWithResponse(dynamic Request);
