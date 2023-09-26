using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace WorkflowHandlers.RequestHandlers;

public class CommandRequestHandler : IRequestHandler<CommandRequest, Unit>
{
    private readonly ILogger<CommandRequestHandler> _logger;

    public CommandRequestHandler(ILogger<CommandRequestHandler> logger)
    {
        _logger = logger;
    }

    public Task<Unit> Handle(CommandRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CommandRequest: {description}", request.Description);

        return Task.FromResult(Unit.Value);
    }
}
