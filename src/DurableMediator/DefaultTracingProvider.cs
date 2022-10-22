using Microsoft.AspNetCore.Http;

namespace DurableMediator;

internal class DefaultTracingProvider : ITracingProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultTracingProvider(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IDictionary<string, object?> EnrichLogScope(
        Tracing tracing, 
        IDictionary<string, object?> defaultLogScopeParameters)
    {
        defaultLogScopeParameters.Add("traceIdentifier", tracing.TraceIdentifier);

        return defaultLogScopeParameters;
    }

    public Tracing GetTracing() => new Tracing(_httpContextAccessor.HttpContext?.TraceIdentifier);
}
