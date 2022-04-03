using MediatR;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.RequestHandlers;

internal class RequestCHandler : IRequestHandler<RequestC, Unit>
{
    public Task<Unit> Handle(RequestC request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Unit.Value);
    }
}
