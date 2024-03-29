﻿using System.Runtime.CompilerServices;
using DurableMediator.History;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DurableMediator;

internal class WorkflowManagement : IWorkflowManagement
{
    private readonly WorkflowConfiguration _config;
    private readonly HistoryClient _historyClient;
    private readonly IDurableClientFactory _durableClientFactory;

    public WorkflowManagement(
        IOptions<WorkflowConfiguration> config,
        HistoryClient historyClient,
        IDurableClientFactory durableClientFactory)
    {
        _config = config.Value;
        _historyClient = historyClient;
        _durableClientFactory = durableClientFactory;
    }

    public async Task<DetailedWorkflowStatus<JToken, JToken?>?> GetWorkflowAsync(string instanceId)
    {
        var history = await _historyClient.GetDurableOrchestrationStatusAsync(instanceId).ConfigureAwait(false);

        var status = await GetClient().GetStatusAsync(instanceId).ConfigureAwait(false);

        return MapDetails(status, history);
    }

    public async Task<DetailedWorkflowStatus<TRequest>?> GetWorkflowAsync<TRequest>(string instanceId)
        where TRequest : IWorkflowRequest
    {
        var history = await _historyClient.GetDurableOrchestrationStatusAsync(instanceId).ConfigureAwait(false);

        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId, false).ConfigureAwait(false);

        return MapDetails<TRequest>(status, history);
    }

    public async Task<DetailedWorkflowStatus<TRequest, TResponse>?> GetWorkflowAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>
    {
        var history = await _historyClient.GetDurableOrchestrationStatusAsync(instanceId).ConfigureAwait(false);

        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId, false).ConfigureAwait(false);

        return MapDetails<TRequest, TResponse>(status, history);
    }

    public async Task<TRequest?> GetWorkflowDataAsync<TRequest>(string instanceId)
        where TRequest : IWorkflowRequest
    {
        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId, showHistory: false).ConfigureAwait(false);
        if (status == null)
        {
            return default;
        }

        return status.Input.ToObject<TRequest>();
    }

    public async Task<(TRequest? request, TResponse? response)> GetWorkflowDataAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>
    {
        var status = await GetOrchestrationStatusAsync<TRequest>(instanceId, showHistory: false).ConfigureAwait(false);
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

    public async IAsyncEnumerable<WorkflowStatus<JToken, JToken?>> GetRecentWorkflowsAsync(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var item in GetWorkflowStatusAsync(instanceIdPrefix, status => true, token).ConfigureAwait(false))
        {
            if (Map(item) is { } status)
            {
                yield return status;
            }
        }
    }

    public async IAsyncEnumerable<WorkflowStatus<TRequest>> GetRecentWorkflowsAsync<TRequest>(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
        where TRequest : IWorkflowRequest
    {
        await foreach (var item in GetWorkflowStatusAsync(instanceIdPrefix, IsInvocationOfType<TRequest>, token).ConfigureAwait(false))
        {
            if (Map<TRequest>(item) is { } status)
            {
                yield return status;
            }
        }
    }

    public async IAsyncEnumerable<WorkflowStatus<TRequest, TResponse>> GetRecentWorkflowsAsync<TRequest, TResponse>(string instanceIdPrefix, [EnumeratorCancellation] CancellationToken token)
        where TRequest : IWorkflowRequest<TResponse>
    {
        await foreach (var item in GetWorkflowStatusAsync(instanceIdPrefix, IsInvocationOfType<TRequest>, token).ConfigureAwait(false))
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
            InstanceIdPrefix = instanceIdPrefix,
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

    private async Task<DurableOrchestrationStatus?> GetOrchestrationStatusAsync<TRequest>(string instanceId, bool showHistory)
    {
        var status = await GetClient().GetStatusAsync(instanceId,
            showHistory: showHistory,
            showHistoryOutput: showHistory,
            showInput: showHistory).ConfigureAwait(false);
        if (status == null || !IsInvocationOfType<TRequest>(status))
        {
            return default;
        }

        return status;
    }

    private async IAsyncEnumerable<DurableOrchestrationStatus> GetWorkflowStatusAsync(string instanceIdPrefix, Func<DurableOrchestrationStatus, bool> selector, [EnumeratorCancellation] CancellationToken token)
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
                InstanceIdPrefix = instanceIdPrefix,
                PageSize = 100,
                ContinuationToken = continueToken
            }, token).ConfigureAwait(false);

            day.AddRange(result.DurableOrchestrationState.Where(x => !x.InstanceId.StartsWith('@')).Where(selector));

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

    private static DetailedWorkflowStatus<JToken, JToken?>? MapDetails(DurableOrchestrationStatus? status, JArray? history)
    {
        if (status == null)
        {
            return null;
        }

        var (input, state) = GetInputAndState<JToken>(status);

        return new DetailedWorkflowStatus<JToken, JToken?>(
            input.Request.Value<string>(nameof(IWorkflowRequest.WorkflowName)),
            status.InstanceId,
            Map(status.RuntimeStatus),
            input.Request,
            status.Output,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage,
            history);
    }

    private static DetailedWorkflowStatus<TRequest>? MapDetails<TRequest>(DurableOrchestrationStatus? status, JArray? history)
        where TRequest : IWorkflowRequest
    {
        if (status == null)
        {
            return null;
        }

        var (input, state) = GetInputAndState<TRequest>(status);

        return new DetailedWorkflowStatus<TRequest>(
            input.Request.WorkflowName,
            status.InstanceId,
            Map(status.RuntimeStatus),
            input.Request,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage,
            history);
    }

    private static DetailedWorkflowStatus<TRequest, TResponse>? MapDetails<TRequest, TResponse>(DurableOrchestrationStatus? status, JArray? history)
        where TRequest : IWorkflowRequest<TResponse>
    {
        if (status == null)
        {
            return null;
        }

        var (input, state) = GetInputAndState<TRequest>(status);
        var output = status.Output == null ? default : status.Output.ToObject<TResponse>();

        return new DetailedWorkflowStatus<TRequest, TResponse>(
            input.Request.WorkflowName,
            status.InstanceId,
            Map(status.RuntimeStatus),
            input.Request,
            output,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage,
            history);
    }

    private static WorkflowStatus<JToken, JToken?>? Map(DurableOrchestrationStatus? status)
    {
        if (status == null)
        {
            return null;
        }

        var (input, state) = GetInputAndState<JToken>(status);

        return new WorkflowStatus<JToken, JToken?>(
            input.Request.Value<string>(nameof(IWorkflowRequest.WorkflowName)),
            status.InstanceId,
            Map(status.RuntimeStatus),
            input.Request,
            status.Output,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage);
    }

    private static WorkflowStatus<TRequest>? Map<TRequest>(DurableOrchestrationStatus? status)
        where TRequest : IWorkflowRequest
    {
        if (status == null)
        {
            return null;
        }

        var (input, state) = GetInputAndState<TRequest>(status);

        return new WorkflowStatus<TRequest>(
            input.Request.WorkflowName,
            status.InstanceId,
            Map(status.RuntimeStatus),
            input.Request,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage);
    }

    private static WorkflowStatus<TRequest, TResponse>? Map<TRequest, TResponse>(DurableOrchestrationStatus? status)
        where TRequest : IWorkflowRequest<TResponse>
    {
        if (status == null)
        {
            return null;
        }

        var (input, state) = GetInputAndState<TRequest>(status);
        var output = status.Output == null ? default : status.Output.ToObject<TResponse>();

        return new WorkflowStatus<TRequest, TResponse>(
            input.Request.WorkflowName,
            status.InstanceId,
            Map(status.RuntimeStatus),
            input.Request,
            output,
            status.CreatedTime,
            status.LastUpdatedTime,
            state?.ExceptionMessage);
    }
    private static (WorkflowRequestWrapper<TRequest> input, WorkflowErrorState state) GetInputAndState<TRequest>(DurableOrchestrationStatus status)
        => (status.Input.ToObject<WorkflowRequestWrapper<TRequest>>(), status.CustomStatus.ToObject<WorkflowErrorState>());

    private static WorkflowRuntimeStatus Map(OrchestrationRuntimeStatus status)
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

    public async Task RestartWorkflowAsync(string instanceId)
    {
        await GetClient().RestartAsync(instanceId, restartWithNewInstanceId: false);
    }

    public async Task RewindWorkflowAsync(string instanceId)
    {
        await GetClient().RewindAsync(instanceId, $"Rewound by {nameof(WorkflowManagement)}");
    }
}
