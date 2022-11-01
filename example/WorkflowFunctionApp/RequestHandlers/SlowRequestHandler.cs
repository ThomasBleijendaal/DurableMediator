using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp.Requests;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.RequestHandlers;

internal class SlowRequestHandler : IRequestHandler<SlowRequest, BasicResponse>
{
    private readonly ILogger<SlowRequestHandler> _logger;

    public SlowRequestHandler(ILogger<SlowRequestHandler> logger)
    {
        _logger = logger;
    }

    public async Task<BasicResponse> Handle(SlowRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing SlowRequest");

        await Task.Delay(10000, cancellationToken);

        return new BasicResponse(Guid.NewGuid());
    }
}
