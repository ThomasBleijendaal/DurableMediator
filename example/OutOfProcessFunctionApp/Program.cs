using MediatR;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {

    })
    .ConfigureServices(services =>
    {
        services.AddMediatR(typeof(Program).Assembly);
    })
    .Build();

host.Run();
