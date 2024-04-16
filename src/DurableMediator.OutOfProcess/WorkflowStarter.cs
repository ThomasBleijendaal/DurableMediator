using Microsoft.DurableTask.Client;

namespace DurableMediator.OutOfProcess;

internal class WorkflowStarter : IWorkflowStarter
{
    private readonly IDurableTaskClientProvider _provider;

    public WorkflowStarter(IDurableTaskClientProvider provider)
    {
        _provider = provider;
    }

    public Task<string> StartWorkflowAsync<TWorkflowRequest>(TWorkflowRequest workflowRequest) where TWorkflowRequest : IWorkflowRequest
    {
        return _provider.GetClient(nameof(DurableMediator)).StartWorkflowAsync(workflowRequest);
    }
}
