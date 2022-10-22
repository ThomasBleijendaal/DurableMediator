using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.RequestHandlers;

internal class FailingRequestHandler : IRequestHandler<FailingRequest, BasicResponse>
{
    private readonly ILogger<FailingRequestHandler> _logger;

    public FailingRequestHandler(ILogger<FailingRequestHandler> logger)
    {
        _logger = logger;
    }

    public Task<BasicResponse> Handle(FailingRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing FailingRequest");

        if (Random.Shared.Next(1, 10) < 9)
        {
            throw new InvalidOperationException("FailingRequest failed");
        }

        return Task.FromResult(new BasicResponse(Guid.NewGuid()));
    }
}
