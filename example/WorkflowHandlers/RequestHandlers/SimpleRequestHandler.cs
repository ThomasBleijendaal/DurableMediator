using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.RequestHandlers;

public class SimpleRequestHandler : IRequestHandler<SimpleRequest, BasicResponse>
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
