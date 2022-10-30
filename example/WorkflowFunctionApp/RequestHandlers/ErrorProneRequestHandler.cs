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

        var isSuccess = Random.Shared.Next(1, 10) < 5;

        if (!isSuccess)
        {
            _logger.LogWarning("ErrorProneRequest failed!");

            throw new InvalidOperationException("Error prone handler failed");
        }
        else
        {
            _logger.LogInformation("ErrorProneRequest succeeded!");
        }

        return Task.FromResult(new SuccessResponse(Guid.NewGuid()));
    }
}
