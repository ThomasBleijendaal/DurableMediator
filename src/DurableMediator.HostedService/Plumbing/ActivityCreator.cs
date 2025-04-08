using DurableMediator.HostedService.Constants;
using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DurableMediator.HostedService.Plumbing;

internal class ActivityCreator<TActivity> : ObjectCreator<TaskActivity>
    where TActivity : TaskActivity, IActivity
{
    private readonly IServiceProvider _serviceProvider;

    public ActivityCreator(IServiceProvider serviceProvider)
    {
        Name = TActivity.Name;
        Version = Versions.Default;

        _serviceProvider = serviceProvider;
    }

    public override TaskActivity Create() => _serviceProvider.GetRequiredService<TActivity>();
}
