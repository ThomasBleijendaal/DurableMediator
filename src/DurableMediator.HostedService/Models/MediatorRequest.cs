using System.Text.Json.Serialization;
using MediatR;

namespace DurableMediator.HostedService.Models;

[JsonConverter(typeof(MediatorRequestJsonConverter))]
public record MediatorRequest(IBaseRequest Request);
