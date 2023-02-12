using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowHandlers.Requests;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<SimpleRequest>());
    })
    .Build();

host.Run();
