using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Converters;
using Microsoft.Azure.Functions.Worker.Core;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OutOfProcessFunctionApp;

[assembly: WorkerExtensionStartup(typeof(TestExtension))]

namespace OutOfProcessFunctionApp;

public class Function1
{
    private readonly ILogger _logger;

    public Function1(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<Function1>();
    }

    [Function(nameof(StartHelloCitiesUntypedAsync))]
    public async Task<HttpResponseData> StartHelloCitiesUntypedAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(StartHelloCitiesUntypedAsync));

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(HelloCitiesUntypedAsync));
        logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

        return client.CreateCheckStatusResponse(req, instanceId);
    }

    [Function(nameof(HelloCitiesUntypedAsync))]
    public static async Task<string> HelloCitiesUntypedAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var result = "";
        result += await context.CallActivityAsync<string>(nameof(SayHelloUntyped), "Tokyo") + " ";
        result += await context.CallActivityAsync<string>(nameof(SayHelloUntyped), "London") + " ";
        result += await context.CallActivityAsync<string>(nameof(SayHelloUntyped), "Seattle");
        return result;
    }

    [Function(nameof(SayHelloUntyped))]
    public static string SayHelloUntyped([ActivityTrigger] string cityName, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(SayHelloUntyped));
        logger.LogInformation("Saying hello to {name}", cityName);
        return $"Hello, {cityName}!";
    }
}

public class TestExtension : WorkerExtensionStartup
{
    public override void Configure(IFunctionsWorkerApplicationBuilder applicationBuilder)
    {
        Console.WriteLine("YEAH");

        applicationBuilder.Services.Configure<WorkerOptions>((workerOption) =>
        {
            workerOption.InputConverters.Register<InputBinding>();
        });
    }
}

public class InputBinding : IInputConverter
{
    public ValueTask<ConversionResult> ConvertAsync(ConverterContext context)
    {
        throw new NotImplementedException();
    }
}
