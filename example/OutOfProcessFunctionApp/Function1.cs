using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace IsolatedFunctionApp1.Typed;

public static class HelloCitiesTypedStarter
{
    [Function(nameof(StartHelloCitiesTypedAsync))]
    public static async Task<HttpResponseData> StartHelloCitiesTypedAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(StartHelloCitiesTypedAsync));

        var instanceId = await client.ScheduleNewHelloCitiesTypedInstanceAsync();
        logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

        return client.CreateCheckStatusResponse(req, instanceId);
    }
}

[DurableTask(nameof(HelloCitiesTyped))]
public class HelloCitiesTyped : TaskOrchestrator<string?, string>
{
    public async override Task<string> RunAsync(TaskOrchestrationContext context, string? input)
    {
        var result = "";
        result += await context.CallSayHelloTypedAsync("Tokyo") + " ";
        result += await context.CallSayHelloTypedAsync("London") + " ";
        result += await context.CallSayHelloTypedAsync("Seattle");
        return result;
    }
}

[DurableTask(nameof(SayHelloTyped))]
public class SayHelloTyped : TaskActivity<string, string>
{
    readonly ILogger? _logger;

    public SayHelloTyped(ILoggerFactory? loggerFactory)
    {
        this._logger = loggerFactory?.CreateLogger<SayHelloTyped>();
    }

    public override Task<string> RunAsync(TaskActivityContext context, string cityName)
    {
        this._logger?.LogInformation("Saying hello to {name}", cityName);
        return Task.FromResult($"Hello, {cityName}!");
    }
}
