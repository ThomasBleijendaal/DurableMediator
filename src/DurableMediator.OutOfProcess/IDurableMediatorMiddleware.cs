namespace DurableMediator.OutOfProcess;

public interface IDurableMediatorMiddleware
{
    Task PreProcessAsync<TRequest>(TRequest request, string instanceId);

    Task PostProcessAsync<TRequest, TResponse>(TRequest request, TResponse response, string instanceId);
}

