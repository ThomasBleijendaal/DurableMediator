using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.RequestHandlers;

internal class SimpleRequestHandler : IRequestHandler<SimpleRequest, BasicResponse>
{
    private readonly ILogger<SimpleRequestHandler> _logger;

    public SimpleRequestHandler(ILogger<SimpleRequestHandler> logger)
    {
        _logger = logger;
    }

    public Task<BasicResponse> Handle(SimpleRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing SimpleRequest: {description}", request.Description);

        return Task.FromResult(new BasicResponse(Guid.NewGuid()));
    }
}
