using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;

namespace DurableMediator;

public class DurableMediatorFunctionProvider : IFunctionProvider
{
    public ImmutableDictionary<string, ImmutableArray<string>> FunctionErrors { get; } = new Dictionary<string, ImmutableArray<string>>().ToImmutableDictionary();

    public Task<ImmutableArray<FunctionMetadata>> GetFunctionMetadataAsync()
    {
        var functionMetadataList = GetFunctionMetadataList();

        return Task.FromResult(functionMetadataList.ToImmutableArray());
    }

    private List<FunctionMetadata> GetFunctionMetadataList()
    {
        var list = new List<FunctionMetadata>
        {
            GetDurableMediatorFunctionMetadata()
        };

        return list;
    }

    private FunctionMetadata GetDurableMediatorFunctionMetadata()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var functionMetadata = new FunctionMetadata()
        {
            Name = nameof(DurableMediatorEntity),
            FunctionDirectory = null,
            ScriptFile = $"assembly:{assembly.FullName}",
            EntryPoint = $"{assembly.GetName().Name}.{nameof(DurableMediatorFunction)}.{nameof(DurableMediatorFunction.RunAsync)}",
            Language = "DotNetAssembly"
        };

        var jo = JObject.FromObject(new EntityTriggerMetadata());
        var binding = BindingMetadata.Create(jo);
        functionMetadata.Bindings.Add(binding);

        return functionMetadata;
    }
}
