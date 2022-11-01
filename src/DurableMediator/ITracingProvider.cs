namespace DurableMediator;

public interface ITracingProvider
{
    /// <summary>
    /// This method provides a Tracing object (or derivative) that is used to enrich log scopes.
    /// </summary>
    /// <returns></returns>
    Tracing GetTracing();

    /// <summary>
    /// This method transforms the given Tracing object (or derivative) and default log scope 
    /// parameters to produce an enriched set of log scope parameters that is used 
    /// to correlate log messages.
    /// </summary>
    /// <param name="tracing"></param>
    /// <param name="defaultLogScopeParameters"></param>
    /// <returns></returns>
    IDictionary<string, object?> EnrichLogScope(Tracing tracing, IDictionary<string, object?> defaultLogScopeParameters);
}
