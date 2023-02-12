using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    })
    .Build();

host.Run();
