using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.RequestHandlers;

internal class RequestBHandler : IRequestHandler<RequestB, RequestBResponse>
{
    private readonly ILogger<RequestBHandler> _logger;

    public RequestBHandler(ILogger<RequestBHandler> logger)
    {
        _logger = logger;
    }

    public Task<RequestBResponse> Handle(RequestB request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing RequestB");

        return Task.FromResult(new RequestBResponse(Random.Shared.NextDouble() < 0.3));
    }
}
