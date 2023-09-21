using System.Text.Json;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMediator.OutOfProcess;

public class OrchestratorExecutor<TRequest, TResponse> : IWorkflowExecution<TRequest>
{
    private readonly TaskOrchestrationContext _context;
    private readonly TRequest _request;
    private readonly ILogger _logger;

    public OrchestratorExecutor(TaskOrchestrationContext context, TRequest request, ILogger logger)
    {
        _context = context;
        _request = request;
        _logger = logger;
    }

    public TaskOrchestrationContext OrchestrationContext => _context;

    public ILogger ReplaySafeLogger => _logger;

    public TRequest Request => _request;

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await _context.CallDurableMediatorAsync(new MediatorRequest((IRequest<Unit>)request));

            return default!;
        }

        var response = await _context.CallDurableMediatorWithResponseAsync(new MediatorRequestWithResponse(request));

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }

    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowRequest, TWorkflowResponse>(TWorkflowRequest request)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>
    {
        return _context.CallSubOrchestratorAsync<TWorkflowResponse?>(TWorkflowRequest.Workflow.Name, request, null);
    }
}
