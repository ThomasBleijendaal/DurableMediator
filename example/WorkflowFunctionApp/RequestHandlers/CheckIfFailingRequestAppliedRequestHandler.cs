using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.RequestHandlers;

internal class CheckIfFailingRequestAppliedRequestHandler : IRequestHandler<CheckIfFailingRequestAppliedRequest, BasicResponse?>
{
    private readonly ILogger<CheckIfFailingRequestAppliedRequestHandler> _logger;

    public CheckIfFailingRequestAppliedRequestHandler(ILogger<CheckIfFailingRequestAppliedRequestHandler> logger)
    {
        _logger = logger;
    }

    public async Task<BasicResponse?> Handle(CheckIfFailingRequestAppliedRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CheckIfFailingRequestAppliedRequest");

        await Task.Delay(10000, cancellationToken);

        if (Random.Shared.Next(1, 10) < 3)
        {
            return null;
        }

        return new BasicResponse(Guid.NewGuid());
    }
}
