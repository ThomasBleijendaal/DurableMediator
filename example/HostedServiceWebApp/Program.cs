using DurableMediator.HostedService;
using DurableMediator.HostedService.Extensions;
using DurableTask.AzureStorage;
using HostedServiceWebApp.Extensions;
using HostedServiceWebApp.Middlewares;
using HostedServiceWebApp.Workflows;
using Microsoft.AspNetCore.Mvc;
using WorkflowHandlers.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureStorageOrchestrationService(useAppLease: false);

// adds IWorkflowService to start workflows
builder.Services.AddWorkflowService(sp => sp.GetRequiredService<AzureStorageOrchestrationService>());

// adds workflow infrastructure to run workflows
builder.Services.AddDurableMediator(sp => sp.GetRequiredService<AzureStorageOrchestrationService>());

builder.Services.AddWorkflow<BasicWorkflow>();
builder.Services.AddWorkflow<RecoveringWorkflow>();
builder.Services.AddWorkflow<ResilientWorkflow>();
builder.Services.AddWorkflow<ReusableWorkflow>();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<SimpleRequest>());

builder.Services.AddTransient<IDurableMediatorMiddleware, LogAllRequestsMiddleware>();

var app = builder.Build();

app.MapPost("basic", async ([FromServices] IWorkflowService workflowService) =>
{
    var instanceId = await workflowService.StartWorkflowAsync(new BasicWorkflowRequest(Guid.NewGuid()));
    return Results.Accepted(null, instanceId);
});

app.MapPost("recovering", async ([FromServices] IWorkflowService workflowService) =>
{
    var instanceId = await workflowService.StartWorkflowAsync(new RecoveringWorkflowRequest(Guid.NewGuid()));
    return Results.Accepted(null, instanceId);
});

app.MapPost("resilient", async ([FromServices] IWorkflowService workflowService) =>
{
    var instanceId = await workflowService.StartWorkflowAsync(new ResilientWorkflowRequest(Guid.NewGuid()));
    return Results.Accepted(null, instanceId);
});

app.Run();
