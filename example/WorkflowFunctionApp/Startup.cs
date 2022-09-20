using System.Runtime.CompilerServices;
using DurableMediator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkflowFunctionApp;
using WorkflowFunctionApp.Workflows;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: InternalsVisibleTo("DurableMediator")]
namespace WorkflowFunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
        });

        builder.AddDurableMediator(typeof(Startup));
    }
}
