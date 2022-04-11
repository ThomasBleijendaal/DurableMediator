using System.Runtime.CompilerServices;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Options;

namespace DurableMediator;

internal class WorkflowMonitor : IWorkflowMonitor
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

    public async Task<WorkflowStatus?> GetWorkflowAsync(string instanceId)
    {
        var status = await GetOrchestrationStatusAsync(instanceId).ConfigureAwait(false);

        return Map(status);
    }

    public async Task<(TRequest request, TResponse? response)> GetWorkflowDataAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>
    {
        var status = await GetOrchestrationStatusAsync(instanceId).ConfigureAwait(false);
        if (status == null || status.Name != typeof(TRequest).Name)
        {
            return default;
        }

        var input = status.Input.ToObject<TRequest>();

        if (status.Output == null)
        {
            return (input, default);
        }

        return (input, status.Output.ToObject<TResponse?>());
    }

    public async IAsyncEnumerable<WorkflowStatus> GetRecentWorkflowsAsync(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
    {
        var client = GetClient();

        var createdTimeFrom = DateTime.UtcNow.AddDays(-1);
        var maxTimeFrom = DateTime.UtcNow.AddDays(-7);

        var continueToken = default(string?);

        var day = new List<DurableOrchestrationStatus>();
        do
        {
            var result = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                CreatedTimeFrom = createdTimeFrom,
                CreatedTimeTo = createdTimeFrom.AddDays(1),
                InstanceIdPrefix = Constants.WorkflowIdPrefix + instanceIdPrefix,
                PageSize = 100,
                ContinuationToken = continueToken
            }, token).ConfigureAwait(false);

            day.AddRange(result.DurableOrchestrationState);

            if (!string.IsNullOrWhiteSpace(result.ContinuationToken))
            {
                continueToken = result.ContinuationToken;
            }
            else
            {
                createdTimeFrom = createdTimeFrom.AddDays(-1);

                foreach (var item in day.OrderByDescending(x => x.CreatedTime))
                {
                    if (Map(item) is WorkflowStatus status)
                    {
                        yield return status;
                    }
                }

                day.Clear();
            }
        }
        while (createdTimeFrom > maxTimeFrom && !token.IsCancellationRequested);
    }

    public async Task<bool> HasRunningTaskAsync(string instanceIdPrefix, CancellationToken token)
    {
        var client = GetClient();

        var result = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
        {
            InstanceIdPrefix = Constants.WorkflowIdPrefix + instanceIdPrefix,
            RuntimeStatus = new[]
            {
                OrchestrationRuntimeStatus.Pending,
                OrchestrationRuntimeStatus.Running
            }
        }, token).ConfigureAwait(false);

        return result.DurableOrchestrationState.Any();
    }

    private IDurableClient GetClient()
        => _durableClientFactory.CreateClient(new DurableClientOptions { TaskHub = _config.HubName });

    private async Task<DurableOrchestrationStatus?> GetOrchestrationStatusAsync(string instanceId)
        => await GetClient().GetStatusAsync(Constants.WorkflowIdPrefix + instanceId).ConfigureAwait(false);

    private WorkflowStatus? Map(DurableOrchestrationStatus? status)
    {
        if (status == null)
        {
            return null;
        }

        var state = status.CustomStatus.ToObject<WorkflowState>();
        return new WorkflowStatus(
            state?.WorkflowName ?? status.InstanceId.Replace(Constants.WorkflowIdPrefix, ""),
            status.InstanceId.Replace(Constants.WorkflowIdPrefix, ""),
            Map(status.RuntimeStatus),
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage);
    }

    private WorkflowRuntimeStatus Map(OrchestrationRuntimeStatus status)
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
}
