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

        return ((JsonElement)wrapper?.Request).Deserialize(wrapper?.Type, options) switch
        {
            IRequest request => new MediatorRequest(request),
            IRequest<Unit> unitRequest => new MediatorRequest(unitRequest),
            IBaseRequest baseRequest => new MediatorRequest(baseRequest),
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, MediatorRequest value, JsonSerializerOptions options)
    {
        var requestObject = value.Request;
        var typeName = requestObject.GetType().AssemblyQualifiedName
            ?? throw new InvalidOperationException($"{nameof(Type.AssemblyQualifiedName)} is null of {requestObject.GetType()}");

        JsonSerializer.Serialize(writer, new MediatorRequestJsonModel(value.Request, typeName), options);
    }
}
