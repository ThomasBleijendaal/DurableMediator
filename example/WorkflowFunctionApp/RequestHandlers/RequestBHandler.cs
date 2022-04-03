using MediatR;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.RequestHandlers;

internal class RequestBHandler : IRequestHandler<RequestB, RequestBResponse>
{
    public Task<RequestBResponse> Handle(RequestB request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RequestBResponse(Random.Shared.NextDouble() < 0.3));
    }
}
