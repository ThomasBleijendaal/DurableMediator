using MediatR;
using Microsoft.Extensions.Logging;
using WorkflowHandlers.Requests;

namespace WorkflowHandlers.RequestHandlers;

public class CommandRequestHandler :
#if NET7_0_OR_GREATER
    IRequestHandler<CommandRequest>
#else
    IRequestHandler<CommandRequest, Unit>
#endif
{
    private readonly ILogger<CommandRequestHandler> _logger;

    public CommandRequestHandler(ILogger<CommandRequestHandler> logger)
    {
        _logger = logger;
    }

    public
#if NET7_0_OR_GREATER
        Task
#else
        Task<Unit>
#endif 
        Handle(CommandRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CommandRequest: {description}", request.Description);

#if NET7_0_OR_GREATER
        return Task.CompletedTask;
#else
        return Unit.Task;
#endif

    }
}
