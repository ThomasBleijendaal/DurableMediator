using MediatR;

namespace DurableMediator;

/// <summary>
/// Represents a workflow requests which will return TResponse as response.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IWorkflowRequest<TResponse>
{
    public string InstanceId { get; }
}

/// <summary>
/// Represents a workflow request without any response.
/// </summary>
public interface IWorkflowRequest : IWorkflowRequest<Unit>
{

}
