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

    public async Task<WorkflowStatus<TRequest>?> GetWorkflowAsync<TRequest>(string instanceId)
    {
        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId).ConfigureAwait(false);

        return Map<TRequest>(status);
    }

    public async Task<WorkflowStatus<TRequest, TResponse>?> GetWorkflowAsync<TRequest, TResponse>(string instanceId)
    {
        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId).ConfigureAwait(false);

        return Map<TRequest, TResponse>(status);
    }

    public async Task<TRequest?> GetWorkflowDataAsync<TRequest>(string instanceId)
        where TRequest : IWorkflowRequest
    {
        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId).ConfigureAwait(false);
        if (status == null)
        {
            return default;
        }

        return status.Input.ToObject<TRequest>();
    }

    public async Task<(TRequest? request, TResponse? response)> GetWorkflowDataAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>
    {
        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId).ConfigureAwait(false);
        if (status == null)
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

    public async IAsyncEnumerable<WorkflowStatus<TRequest>> GetRecentWorkflowsAsync<TRequest>(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var item in GetWorkflowStatusAsync<TRequest>(instanceIdPrefix, token).ConfigureAwait(false))
        {
            if (Map<TRequest>(item) is WorkflowStatus<TRequest> status)
            {
                yield return status;
            }
        }
    }

    public async IAsyncEnumerable<WorkflowStatus<TRequest, TResponse>> GetRecentWorkflowsAsync<TRequest, TResponse>(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var item in GetWorkflowStatusAsync<TRequest>(instanceIdPrefix, token).ConfigureAwait(false))
        {
            if (Map<TRequest, TResponse>(item) is WorkflowStatus<TRequest, TResponse> status)
            {
                yield return status;
            }
        }
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

    private static bool IsInvocationOfType<TRequest>(DurableOrchestrationStatus status)
        => status.Name == typeof(TRequest).Name;

    private async Task<DurableOrchestrationStatus?> GetOrchestrationStatusAsync<TRequest>(string instanceId)
    {
        var status = await GetClient().GetStatusAsync(Constants.WorkflowIdPrefix + instanceId).ConfigureAwait(false);
        if (status == null || !IsInvocationOfType<TRequest>(status))
        {
            return default;
        }

        return status;
    }

    private async IAsyncEnumerable<DurableOrchestrationStatus> GetWorkflowStatusAsync<TRequest>(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
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

            day.AddRange(result.DurableOrchestrationState.Where(IsInvocationOfType<TRequest>));

            if (!string.IsNullOrWhiteSpace(result.ContinuationToken))
            {
                continueToken = result.ContinuationToken;
            }
            else
            {
                createdTimeFrom = createdTimeFrom.AddDays(-1);

                foreach (var item in day.OrderByDescending(x => x.CreatedTime))
                {
                    yield return item;
                }

                day.Clear();
            }
        }
        while (createdTimeFrom > maxTimeFrom && !token.IsCancellationRequested);
    }

    private WorkflowStatus<TRequest>? Map<TRequest>(DurableOrchestrationStatus? status)
    {
        if (status == null)
        {
            return null;
        }

        var state = status.CustomStatus.ToObject<WorkflowState>();

        var input = status.Input.ToObject<TRequest>();

        return new WorkflowStatus<TRequest>(
            state?.WorkflowName ?? status.InstanceId.Replace(Constants.WorkflowIdPrefix, ""),
            status.InstanceId.Replace(Constants.WorkflowIdPrefix, ""),
            Map(status.RuntimeStatus),
            input,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage);
    }

    private WorkflowStatus<TRequest, TResponse>? Map<TRequest, TResponse>(DurableOrchestrationStatus? status)
    {
        if (status == null)
        {
            return null;
        }

        var state = status.CustomStatus.ToObject<WorkflowState>();

        var input = status.Input.ToObject<TRequest>();
        var output = status.Output == null ? default : status.Output.ToObject<TResponse>();

        return new WorkflowStatus<TRequest, TResponse>(
            state?.WorkflowName ?? status.InstanceId.Replace(Constants.WorkflowIdPrefix, ""),
            status.InstanceId.Replace(Constants.WorkflowIdPrefix, ""),
            Map(status.RuntimeStatus),
            input,
            output,
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
