using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Options;

namespace DurableMediator
{
    public class WorkflowMonitor : IWorkflowMonitor
    {
        private readonly WorkflowConfiguration _config;
        private readonly IDurableClientFactory _durableClientFactory;

        public WorkflowMonitor(
            IOptions<WorkflowConfiguration> config,
            IDurableClientFactory durableClientFactory)
        {
            _config = config.Value;
            _durableClientFactory = durableClientFactory;
        }

        public async Task<WorkflowStatus> GetWorkflowAsync(string id)
        {
            var client = GetClient();

            var status = await client.GetStatusAsync(id);

            return Map(status);
        }

        public async Task<IReadOnlyList<WorkflowStatus>> GetRecentWorkflowsAsync(string instanceIdPrefix)
        {
            var client = GetClient();

            var result = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                CreatedTimeFrom = DateTime.UtcNow.AddDays(-7),
                InstanceIdPrefix = instanceIdPrefix

            }, CancellationToken.None);

            return result.DurableOrchestrationState.Select(Map).ToList();
        }

        public async Task<bool> HasRunningTaskAsync(string instanceIdPrefix, Guid affectedEntityId)
        {
            var client = GetClient();

            var result = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = instanceIdPrefix,
                RuntimeStatus = new[]
                {
                    OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Running
                }
            }, CancellationToken.None);

            return false; // TODO: result.DurableOrchestrationState.Select(Map); // TODO: .Any(x => x.AffectedEntityId == affectedEntityId);
        }

        private IDurableClient GetClient()
            => _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

        private WorkflowStatus Map(DurableOrchestrationStatus status)
        {
            var state = status.CustomStatus.ToObject<WorkflowState>();
            return new WorkflowStatus(
                state?.WorkflowName ?? status.InstanceId,
                status.InstanceId,
                Map(status?.RuntimeStatus));
        }

        private WorkflowRuntimeStatus Map(OrchestrationRuntimeStatus? status)
            => status switch
            {
                OrchestrationRuntimeStatus.Unknown => WorkflowRuntimeStatus.Unknown,
                OrchestrationRuntimeStatus.Running => WorkflowRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Completed => WorkflowRuntimeStatus.Completed,
                OrchestrationRuntimeStatus.ContinuedAsNew => WorkflowRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Failed => WorkflowRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Canceled => WorkflowRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Terminated => WorkflowRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Pending => WorkflowRuntimeStatus.Pending,
                _ => WorkflowRuntimeStatus.Unknown
            };

        private string MapStatus(DurableOrchestrationStatus status)
            => $"[{status.InstanceId}: {status.CustomStatus.ToObject<WorkflowState>()?.ExceptionMessage}";
    }
}
