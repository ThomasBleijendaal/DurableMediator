namespace OutOfProcessFunctionApp.Tests.Base;

internal class ScenarioRun : IScenarioRun
{
    private readonly GuidGenerator _guidGenerator = new();

    public DateTime CurrentUtcDateTime { get; private set; } = DateTime.UtcNow;

    public void AddTick()
    {
        CurrentUtcDateTime += TimeSpan.FromSeconds(1);
    }

    public Guid NewGuid() => _guidGenerator.GetGuid();
}
