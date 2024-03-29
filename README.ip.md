# Durable Mediator

## Azure Functions Out-Of-Process

[![#](https://img.shields.io/nuget/vpre/DurableMediator.OutOfProcess?style=flat-square)](https://www.nuget.org/packages/DurableMediator.OutOfProcess)

[Readme for Azure Functions Out-Of-Process (.NET 7 and higher)](README.oop.md).

## Azure Functions In-Process

[![#](https://img.shields.io/nuget/v/DurableMediator?style=flat-square)](https://www.nuget.org/packages/DurableMediator)

Durable Mediator is an extension to the Durable Task library which allows for running MediatR Requests as activities in orchestrations without any complex ceremony.

## Blog post

https://medium.com/tripleuniverse/how-to-effectively-work-with-stateful-processes-in-a-stateless-environment-ae4d014d5606

## Getting started

To get started with running orchestrations which call MediatR Requests as activities, which are called "Workflows", follow these steps:

First start by defining a workflow request:

```c#
public record WorkflowRequest(Guid SomeId) : IWorkflowRequest
{
    public string WorkflowName => "Human readable workflow name";
    public string InstanceId => SomeId.ToString();
};
```

Also, create a `IRequest` that needs to be called from the workflow:

```c#
public record MediatorRequest(Guid SomeId) : IRequest;
```

Create a request handler that handles this MediatR request:

```c#
public class MediatorRequestHandler : IRequestHandler<MediatorRequest>
{
    private readonly ILogger<MediatorRequestHandler> _logger;

    public RequestAHandler(ILogger<MediatorRequestHandler> logger)
    {
        _logger = logger;
    }

    public Task<Unit> Handle(MediatorRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing RequestA");

        return Task.FromResult(Unit.Value);
    }
}

```

Create a workflow that handles the `WorkflowRequest`:

```c#
public class ExampleWorkflow : IWorkflow<WorkflowRequest, Unit>
{
    private readonly ILogger<ABCWorkflow> _logger;

    public ExampleWorkflow(ILogger<ABCWorkflow> logger) 
    {
        _logger = logger;
    }

    public async Task<Unit> OrchestrateAsync(IWorkflowExecution<WorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(_logger);

        logger.LogInformation("Start with workflow");

        await execution.ExecuteAsync(new MediatorRequest(execution.Request.SomeId));

        logger.LogInformation("Workflow done");

        return Unit.Value;
    }
}
```

Create a Azure Function that triggers this workflow:

```c#
public static class WorkflowTrigger
{
    [FunctionName(nameof(WorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workflow")] HttpRequestMessage req,
        [Workflow] IWorkflowStarter starter)
    {
        var startDetails = await starter.StartNewAsync(new WorkflowRequest(Guid.NewGuid()));

        return new AcceptedResult("", startDetails);
    }
}
```

In your function app startup, add the required services by including `builder.AddDurableMediator(typeof(Startup));`.

When running your function app you will see that next to the WorkflowTrigger Http Trigger function, an 
Orchestration Trigger function called "WorkflowRequest" and a "DurableMediatorEntity" Entity Trigger 
function are added. When http function triggers the start of the workflow, the orchestration function 
will orchestrate the workflow and invoke `ExampleWorkflow`. Each call to the `execution.ExecuteAsync` 
will trigger the DurableMediatorEntity to execute the MediatR Request in a separate activity after which 
the orchestration resumes. No more calling `context.CallActivity` and guessing what parameters to pass in.

The `execution` exposes the `IDurableOrchestrationContext` from the Durable Task library, giving full access 
to creating timers for durable delays, locking entities for critical sections, or wait for external events. 
The `execution` also exposes methods from `ISubWorkflowOrchestrator`, which allows workflows to initiate other workflows,
and even await their responses, making it easy to compose workflows. 

## Examples

See the WorkflowFunctionApp in the example folder for more examples.

## Preview: Activity history and durable entities

Use `builder.AddDurableMediator(useExperimentalEntityExecution: true, typeof(Startup));` to enable entity execution
which allows for:

- Enable capturing request and response data to and from durable mediator invocations. This allows 
`IWorkflowManagement.GetWorkflowAsync` to output that data, making it possible to access the exact requests 
and responses after the workflow has completed.

The downside of this way feature is that every workflow will create a durable entity history record. This record
will be stored in the orchestration history storage (under `@durablemediator@{instanceId}`) and needs to be cleared
using `IDurableEntityClient.CleanEntityStorageAsync`.
