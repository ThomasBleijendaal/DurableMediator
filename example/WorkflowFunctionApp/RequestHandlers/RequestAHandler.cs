using MediatR;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.RequestHandlers;

internal class RequestAHandler : IRequestHandler<RequestA, RequestAResponse>
{
    public Task<RequestAResponse> Handle(RequestA request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RequestAResponse(Guid.NewGuid()));
    }
}
