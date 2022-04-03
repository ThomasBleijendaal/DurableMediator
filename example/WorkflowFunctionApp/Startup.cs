﻿using MediatR;
using DurableMediator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using WorkflowFunctionApp;
using WorkflowFunctionApp.Workflows;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: InternalsVisibleTo("DurableMediator")]
namespace WorkflowFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;

            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
            });

            builder.Services.AddMediatR(typeof(Startup));

            builder.AddDurableMediator();
            builder.Services.AddTransient<ExampleWorkflow>();
        }
    }
}