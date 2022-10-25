using System.Collections.Immutable;
using System.Reflection;
using DurableMediator.Functions;
using DurableMediator.Metadata;
using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;

namespace DurableMediator;

internal class DurableMediatorFunctionProvider : IFunctionProvider
{
    private readonly IEnumerable<string> _workflows;

    public DurableMediatorFunctionProvider(IEnumerable<string> workflows)
    {
        _workflows = workflows;
    }

    public ImmutableDictionary<string, ImmutableArray<string>> FunctionErrors { get; }
        = new Dictionary<string, ImmutableArray<string>>().ToImmutableDictionary();

    public Task<ImmutableArray<FunctionMetadata>> GetFunctionMetadataAsync()
    {
        var functionMetadataList = GetFunctionMetadataList();

        return Task.FromResult(functionMetadataList.ToImmutableArray());
    }

    private List<FunctionMetadata> GetFunctionMetadataList()
    {
        var list = new List<FunctionMetadata>
        {
            GetActivityFunctionMetadata(nameof(ActivityFunction.SendObjectAsync)),
            GetActivityFunctionMetadata(nameof(ActivityFunction.SendObjectWithResponseAsync))
        };

        list.AddRange(_workflows.Select(GetOrchestratorFunctionMetadata));

        return list;
    }

    private FunctionMetadata GetActivityFunctionMetadata(string functionName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var functionMetadata = new FunctionMetadata()
        {
            Name = functionName.Replace("Async", ""),
            FunctionDirectory = null,
            ScriptFile = $"assembly:{assembly.FullName}",
            EntryPoint = $"{assembly.GetName().Name}.Functions.{nameof(ActivityFunction)}.{functionName}",
            Language = "DotNetAssembly"
        };

        functionMetadata.Bindings.Add(
            BindingMetadata.Create(
                JObject.FromObject(new ActivityTriggerMetadata())));

        functionMetadata.Bindings.Add(
            BindingMetadata.Create(
                JObject.FromObject(new WorkflowMetadata("executor"))));

        return functionMetadata;
    }

    private FunctionMetadata GetOrchestratorFunctionMetadata(string workflowName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var functionMetadata = new FunctionMetadata()
        {
            Name = workflowName,
            FunctionDirectory = null,
            ScriptFile = $"assembly:{assembly.FullName}",
            EntryPoint = $"{assembly.GetName().Name}.Functions.{nameof(WorkflowOrchestratorFunction)}.{nameof(WorkflowOrchestratorFunction.OrchestarateAsync)}",
            Language = "DotNetAssembly"
        };

        functionMetadata.Bindings.Add(
            BindingMetadata.Create(
                JObject.FromObject(new OrchestrationTriggerMetadata())));

        functionMetadata.Bindings.Add(
            BindingMetadata.Create(
                JObject.FromObject(new WorkflowMetadata("orchestrator"))));

        return functionMetadata;
    }
}
