using System.Text.Json.Serialization;

namespace DurableMediator.HostedService.Models;

[JsonConverter(typeof(WorkflowOutputJsonConverter))]
internal record WorkflowOutput(object? Output);
