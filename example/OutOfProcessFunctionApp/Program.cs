using DurableMediator.OutOfProcess;
using MediatR;
using Microsoft.Extensions.Hosting;
using WorkflowHandlers.Requests;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {

    })
    .ConfigureServices(services =>
    {
        services.AddMediatR(
            typeof(SimpleRequest).Assembly,
            typeof(MediatorRequest).Assembly /* TODO: move to library extension method */);
    })
    .Build();

host.Run();
