using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Exceptions;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.RequestHandlers;

public class CheckIfFailingRequestAppliedRequestHandler : IRequestHandler<CheckIfFailingRequestAppliedRequest, BasicResponse?>
{
    private readonly ILogger<CheckIfFailingRequestAppliedRequestHandler> _logger;

    public CheckIfFailingRequestAppliedRequestHandler(ILogger<CheckIfFailingRequestAppliedRequestHandler> logger)
    {
        _logger = logger;
    }

    public async Task<BasicResponse?> Handle(CheckIfFailingRequestAppliedRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CheckIfFailingRequestAppliedRequest");

        await Task.Delay(100, cancellationToken);

        if (Random.Shared.Next(1, 10) < 3)
        {
            _logger.LogInformation("FailingRequest did not apply");

            return null;
        }
        else if (Random.Shared.Next(1, 10) < 5)
        {
            _logger.LogWarning("Failed to check if FailingRequest applied");

            throw new RequestException("Check failed");
        }

        _logger.LogInformation("FailingRequest applied");

        return new BasicResponse(Guid.NewGuid());
    }
}
