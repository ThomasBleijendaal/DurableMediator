using MediatR;
using Newtonsoft.Json;

namespace DurableMediator;

public interface IWorkflowRequest<TResponse>
{
    public string InstanceId { get; }
}

public interface IWorkflowRequest : IWorkflowRequest<Unit>
{

}

internal record GenericWorkflowRequest(
    string InstanceId,
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)] object Request) : IWorkflowRequest;
