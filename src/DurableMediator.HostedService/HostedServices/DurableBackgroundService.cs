using DurableTask.Core;
using Microsoft.Extensions.Hosting;

namespace DurableMediator.HostedService.HostedServices;

internal class DurableBackgroundService : IHostedService
{
    private readonly TaskHubWorker _taskHubWorker;
    private readonly ObjectCreator<TaskOrchestration>[] _orchestrationCreators;
    private readonly ObjectCreator<TaskActivity>[] _activityCreators;

    public DurableBackgroundService(
        IEnumerable<ObjectCreator<TaskOrchestration>> orchestrationCreators,
        IEnumerable<ObjectCreator<TaskActivity>> activityCreators,
        IOrchestrationService orchestrationService)
    {
        _taskHubWorker = new TaskHubWorker(orchestrationService);
        _orchestrationCreators = [.. orchestrationCreators];
        _activityCreators = [.. activityCreators];
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _taskHubWorker.AddTaskOrchestrations(_orchestrationCreators);
        _taskHubWorker.AddTaskActivities(_activityCreators);

        await _taskHubWorker.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _taskHubWorker.StopAsync();
    }
}
