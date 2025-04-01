using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableMediator.HostedService.Models;

internal class MediatorRequestWithResponseJsonConverter : JsonConverter<MediatorRequestWithResponse>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MediatorRequestWithResponse);

    public override MediatorRequestWithResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<MediatorRequestWithResponseJsonModel>(ref reader, options);

        if (((JsonElement)wrapper?.Request).Deserialize(wrapper?.Type, options) is { } request)
        {
            return new MediatorRequestWithResponse(request);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, MediatorRequestWithResponse value, JsonSerializerOptions options)
    {
        var requestObject = value.Request as object;

        JsonSerializer.Serialize(writer, new MediatorRequestWithResponseJsonModel(value.Request, requestObject.GetType().AssemblyQualifiedName), options);
    }
}
