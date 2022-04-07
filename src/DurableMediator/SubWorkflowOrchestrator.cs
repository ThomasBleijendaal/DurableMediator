using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableMediator;

internal class SubWorkflowOrchestrator : ISubWorkflowOrchestrator
{
    private readonly IDurableOrchestrationContext _context;

    public SubWorkflowOrchestrator(
        IDurableOrchestrationContext context)
    {
        _context = context;
    }

    public Task<TWorkflowResponse?> CallSubWorkflowAsync<TWorkflowResponse>(IWorkflowRequest<TWorkflowResponse> request)
        => _context.CallSubOrchestratorAsync<TWorkflowResponse?>(request.GetType().Name, WorkflowInstanceIdHelper.GetId(request), request);

    public void StartWorkflow(IWorkflowRequest request)
        => _context.StartNewOrchestration(request.GetType().Name, request, WorkflowInstanceIdHelper.GetId(request));
}
