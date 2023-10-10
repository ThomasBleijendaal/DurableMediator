# Durable Mediator

[![#](https://img.shields.io/nuget/v/DurableMediator.OutOfProcess?style=flat-square)](https://www.nuget.org/packages/DurableMediator.OutOfProcess)

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
[DurableTask(nameof(ExampleWorkflow))]
public class ExampleWorkflow : Workflow<WorkflowRequest>
{
    private readonly ILogger<ABCWorkflow> _logger;

    public ExampleWorkflow(ILogger<ABCWorkflow> logger) 
    {
        _logger = logger;
    }

    public override async Task OrchestrateAsync(IWorkflowExecution<WorkflowRequest> execution)
    {
        var logger = execution.OrchestrationContext.CreateReplaySafeLogger(_logger);

        logger.LogInformation("Start with workflow");

        await execution.SendAsync(new MediatorRequest(execution.Request.SomeId));

        logger.LogInformation("Workflow done");
    }
}
```

Create a Azure Function that triggers this workflow:

```c#
public static class WorkflowTrigger
{
    [Function(nameof(WorkflowTrigger))]
    public static async Task<IActionResult> TriggerOrchestratorAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workflow")] HttpRequestMessage req,
        [DurableClient] DurableTaskClient client)
    {
        var startDetails = await client.ScheduleNewExampleWorkflowInstanceAsync(new WorkflowRequest(Guid.NewGuid()));

        var response = req.CreateResponse(HttpStatusCode.Accepted);

        response.WriteString(startDetails);

        return response;
    }
}
```

In your function app startup, add the required MediatR services by including `services.AddMediatR(typeof(MediatorRequest).Assembly);`.

When running your function app you will see that next to the WorkflowTrigger Http Trigger function, an 
Orchestration Trigger function called "WorkflowRequest" and a "DurableMediatorEntity" Entity Trigger 
function are added. When http function triggers the start of the workflow, the orchestration function 
will orchestrate the workflow and invoke `ExampleWorkflow`. Each call to the `execution.SendAsync` 
will trigger the durable mediator action to execute the MediatR Request in a separate activity after which 
the orchestration resumes. No more calling `context.CallActivity` and guessing what parameters to pass in.

The `execution` exposes the `TaskOrchestrationContext` from the Durable Task library, giving full access 
to creating timers for durable delays, locking entities for critical sections, or wait for external events. 
The `execution` also exposes `CallSubWorkflowAsync`, which allows workflows to initiate other workflows,
and even await their responses, making it easy to compose workflows. 

## Unit testing

See the OutOfProcessFunctionApp.Tests in the example folder for how to test workflows using scenarios. A scenario
looks like this:

```c#
public class ExampleWorkflowScenario : Scenario
{
    private readonly Guid _requestId = Guid.NewGuid();

    public override void Setup(IScenarioSetup scenarioSetup, Mock<TaskOrchestrationContext> taskOrchestrationContextMock)
    {
    }

    public override IWorkflowRequest Request => new ExampleWorkflowRequest(_requestId);

    public override IEnumerable<object> RunScenario(IScenarioRun scenarioRun)
    {
        yield return new MediatorRequest(_requestId);
    }
}
```

Based on what the Setup configures and simulates, the `RunScenario` should output what MediatR requests the workflow
should emit. Next to MediatR requests, things like delays, exceptions, calls to other workflows and outputs can be
completely unit tested.


## Examples

See the OutOfProcessFunctionApp in the example folder for more examples.
