using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.RequestHandlers;

internal class ErrorProneRequestHandler : IRequestHandler<ErrorProneRequest, SuccessResponse>
{
    private readonly ILogger<ErrorProneRequestHandler> _logger;

    public ErrorProneRequestHandler(ILogger<ErrorProneRequestHandler> logger)
    {
        _logger = logger;
    }

    public Task<SuccessResponse> Handle(ErrorProneRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing ErrorProneRequest");

        return Task.FromResult(new SuccessResponse(Guid.NewGuid(), Random.Shared.Next(1, 10) < 5));
    }
}
