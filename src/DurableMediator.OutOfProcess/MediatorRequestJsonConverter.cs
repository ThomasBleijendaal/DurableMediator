using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;

namespace DurableMediator.OutOfProcess;

internal class MediatorRequestJsonConverter : JsonConverter<MediatorRequest>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MediatorRequest);

    public override MediatorRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<MediatorRequestJsonModel>(ref reader, options);

        if (((JsonElement)wrapper?.Request).Deserialize(wrapper?.Type, options) is IRequest<Unit> request)
        {
            return new MediatorRequest(request);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, MediatorRequest value, JsonSerializerOptions options)
    {
        var requestObject = value.Request;

        JsonSerializer.Serialize(writer, new MediatorRequestJsonModel(
            value.Request,
            requestObject.GetType().AssemblyQualifiedName), options);
    }
}
