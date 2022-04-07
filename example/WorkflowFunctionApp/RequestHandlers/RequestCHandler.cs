using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;

namespace WorkflowFunctionApp.RequestHandlers;

internal class RequestCHandler : IRequestHandler<RequestC, Unit>
{
    private readonly ILogger<RequestCHandler> _logger;

    public RequestCHandler(ILogger<RequestCHandler> logger)
    {
        _logger = logger;
    }

    public Task<Unit> Handle(RequestC request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing RequestC");

        return Task.FromResult(Unit.Value);
    }
}
