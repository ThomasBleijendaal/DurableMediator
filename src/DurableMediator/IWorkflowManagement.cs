﻿using Newtonsoft.Json.Linq;

namespace DurableMediator;

/// <summary>
/// Gets details of recently executed workflows and allows for restarting and rewinding
/// </summary>
public interface IWorkflowManagement
{
    /// <summary>
    /// Gets the status of the workflow with the given instanceId.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task<DetailedWorkflowStatus<JToken, JToken?>?> GetWorkflowAsync(string instanceId);

    /// <summary>
    /// Gets the status of the workflow with the given instanceId.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task<DetailedWorkflowStatus<TRequest>?> GetWorkflowAsync<TRequest>(string instanceId)
        where TRequest : IWorkflowRequest;

    /// <summary>
    /// Gets the status of the workflow with the given instanceId.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task<DetailedWorkflowStatus<TRequest, TResponse>?> GetWorkflowAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>;

    /// <summary>
    /// Gets the request data of the workflow with the given instanceId.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task<TRequest?> GetWorkflowDataAsync<TRequest>(string instanceId)
        where TRequest : IWorkflowRequest;

    /// <summary>
    /// Gets the request and response data of the workflow with the given instanceId.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task<(TRequest? request, TResponse? response)> GetWorkflowDataAsync<TRequest, TResponse>(string instanceId)
        where TRequest : IWorkflowRequest<TResponse>;

    /// <summary>
    /// Gets the request data of recent workflows.
    /// </summary>
    /// <param name="instanceIdPrefix"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    IAsyncEnumerable<WorkflowStatus<JToken, JToken?>> GetRecentWorkflowsAsync(string instanceIdPrefix, CancellationToken token);

    /// <summary>
    /// Gets the request data of recent workflows.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="instanceIdPrefix"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    IAsyncEnumerable<WorkflowStatus<TRequest>> GetRecentWorkflowsAsync<TRequest>(string instanceIdPrefix, CancellationToken token)
        where TRequest : IWorkflowRequest;

    /// <summary>
    /// Gets the request and response data of recent workflows.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="instanceIdPrefix"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    IAsyncEnumerable<WorkflowStatus<TRequest, TResponse>> GetRecentWorkflowsAsync<TRequest, TResponse>(string instanceIdPrefix, CancellationToken token)
        where TRequest : IWorkflowRequest<TResponse>;

    /// <summary>
    /// Checks whether workflows are active for the given instanceIdPrefix.
    /// </summary>
    /// <param name="instanceIdPrefix"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> HasRunningTaskAsync(string instanceIdPrefix, CancellationToken token);

    /// <summary>
    /// Restarts the workflow.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task RestartWorkflowAsync(string instanceId);

    /// <summary>
    /// Undoes the last failed activity and resumes the workflow.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [Obsolete("Preview. Does not support workflows with retries yet")]
    Task RewindWorkflowAsync(string instanceId);
}
