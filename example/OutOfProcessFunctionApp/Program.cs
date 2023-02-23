using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((context, builder) =>
    {
        
        

    })
    .Build();

host.Run();
