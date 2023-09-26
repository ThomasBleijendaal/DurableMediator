using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableMediator.OutOfProcess;

public class MediatorRequestWithCheckAndResponseJsonConverter : JsonConverter<MediatorRequestWithCheckAndResponse>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MediatorRequestWithCheckAndResponse);

    public override MediatorRequestWithCheckAndResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<MediatorRequestWithCheckAndResponseJsonModel>(ref reader, options);

        if (((JsonElement)wrapper?.Request).Deserialize(wrapper?.Type, options) is { } request &&
            ((JsonElement)wrapper?.CheckIfRequestApplied).Deserialize(wrapper?.CheckType, options) is { } check)
        {
            return new MediatorRequestWithCheckAndResponse(request, check);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, MediatorRequestWithCheckAndResponse value, JsonSerializerOptions options)
    {
        var requestObject = value.Request as object;
        var checkObject = value.CheckIfRequestApplied as object;

        JsonSerializer.Serialize(writer, new MediatorRequestWithCheckAndResponseJsonModel(
            value.Request,
            requestObject.GetType().AssemblyQualifiedName,
            value.CheckIfRequestApplied,
            checkObject.GetType().AssemblyQualifiedName), options);
    }
}
