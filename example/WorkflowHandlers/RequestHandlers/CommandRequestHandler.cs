using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace WorkflowHandlers.RequestHandlers;

public class CommandRequestHandler : IRequestHandler<CommandRequest>
{
    private readonly ILogger<CommandRequestHandler> _logger;

    public CommandRequestHandler(ILogger<CommandRequestHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CommandRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CommandRequest: {description}", request.Description);

        return Task.CompletedTask;
    }
}
