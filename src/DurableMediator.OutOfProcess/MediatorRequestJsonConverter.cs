using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableMediator.OutOfProcess;

public class MediatorRequestJsonConverter : JsonConverter<MediatorRequestWithResponse>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MediatorRequestWithResponse);

    public override MediatorRequestWithResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<MediatorRequestWithResponseJsonModel>(ref reader, options);

        if (wrapper?.Type != null)
        {
            var type = Type.GetType(wrapper.Type);

            if (type != null)
            {
                var request = ((JsonElement)wrapper.Request).Deserialize(type, options);

                if (request != null)
                {
                    return new MediatorRequestWithResponse(request);
                }
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, MediatorRequestWithResponse value, JsonSerializerOptions options)
    {
        var requestObject = value.Request as object;

        JsonSerializer.Serialize(writer, new MediatorRequestWithResponseJsonModel(value.Request, requestObject.GetType().AssemblyQualifiedName), options);
    }
}
