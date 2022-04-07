using Newtonsoft.Json.Linq;

namespace DurableMediator;

internal record GenericWorkflowRequest(
    string InstanceId,
    JObject Request) : IWorkflowRequest
{
    public TRequest GetRequest<TRequest>()
        where TRequest : class
        => Request.ToObject<TRequest>() ?? throw new InvalidOperationException("Cannot restore workflow input");
}
