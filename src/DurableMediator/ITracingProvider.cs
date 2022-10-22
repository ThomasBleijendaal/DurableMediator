namespace DurableMediator;

public interface ITracingProvider
{
    Tracing GetTracing();

    IDictionary<string, object?> EnrichLogScope(Tracing tracing, IDictionary<string, object?> defaultLogScopeParameters);
}
