using MediatR;

namespace DurableMediator;

public interface IWorkflowRequest<TResponse>
{
    public string InstanceId { get; }
}

public interface IWorkflowRequest : IWorkflowRequest<Unit>
{

}
