using DurableMediator.OutOfProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutOfProcessFunctionApp;
using WorkflowHandlers.Requests;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        logging.AddSeq();
    })
    .ConfigureServices(services =>
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<SimpleRequest>());

        services.AddDurableMediator();

        services.AddWorkflowStarter();
        services.AddSyncWorkflowClient();

        services.AddTransient<IDurableMediatorMiddleware, LogAllRequestsMiddleware>();
    })
    .Build();

host.Run();
