using System.Runtime.CompilerServices;
using DurableMediator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: InternalsVisibleTo("DurableMediator")]
namespace WorkflowFunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();

            logging.AddSeq();
        });

        builder.AddDurableMediator(typeof(Startup));
    }
}
