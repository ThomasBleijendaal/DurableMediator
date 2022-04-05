using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.RequestHandlers;

internal class RequestAHandler : IRequestHandler<RequestA, RequestAResponse>
{
    private readonly ILogger<RequestAHandler> _logger;

    public RequestAHandler(ILogger<RequestAHandler> logger)
    {
        _logger = logger;
    }

    public Task<RequestAResponse> Handle(RequestA request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing RequestA");

        return Task.FromResult(new RequestAResponse(Guid.NewGuid()));
    }
}
