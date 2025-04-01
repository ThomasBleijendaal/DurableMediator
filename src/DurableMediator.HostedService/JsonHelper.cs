using System.Text.Json;
using DurableMediator.HostedService;

namespace DurableMediator.HostedService;

internal static class JsonHelper
{
    public static object? Deserialize(this JsonElement jsonObject, string? typeName, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        var type = Type.GetType(typeName);

        if (type == null)
        {
            return null;
        }

        var concreteObject = jsonObject.Deserialize(type, options);

        return concreteObject;
    }
}
