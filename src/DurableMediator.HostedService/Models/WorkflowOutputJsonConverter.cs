using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableMediator.HostedService.Models;

internal class WorkflowOutputJsonConverter : JsonConverter<WorkflowOutput>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(WorkflowOutput);

    public override WorkflowOutput? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<WorkflowOutputJsonModel>(ref reader, options);

        return ((JsonElement)wrapper?.Output).Deserialize(wrapper?.Type, options) is { } obj ? new(obj) : null;
    }

    public override void Write(Utf8JsonWriter writer, WorkflowOutput value, JsonSerializerOptions options)
    {
        var outputObject = value.Output;

        if (outputObject == null)
        {
            JsonSerializer.Serialize(writer, default(object), options);
        }
        else
        {
            var typeName = outputObject.GetType().AssemblyQualifiedName
                ?? throw new InvalidOperationException($"{nameof(Type.AssemblyQualifiedName)} is null of {outputObject.GetType()}");

            JsonSerializer.Serialize(writer, new WorkflowOutputJsonModel(value.Output, typeName), options);
        }
    }
}
