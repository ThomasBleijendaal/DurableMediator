using MediatR;
using Microsoft.DurableTask;

namespace DurableMediator.OutOfProcess;

/*
 *  TODOs:
 * V check if workflow is still picked up once it is inherited
 * X check if workflow can still access activities when its in a nuget package -> NOPE
 * - implement all json converters for all models
 * - add IWorkflowRequest<> to IWorkflowExecution
 * - update MediatR
 * - ConfigureAwait
 * - all the execution specials
 * - merge stuff into abstractions package
 * - make dependency collection extension
 * - add tracing
 * - remove generated extensions and refactor
 * - internalize everything that needs to be internal (also look at example projects)
 */

public class DurableMediator : TaskActivity<MediatorRequest, Unit>
{
    private readonly IMediator _mediator;

    public DurableMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Unit> RunAsync(TaskActivityContext context, MediatorRequest input)
    {
        return await _mediator.Send(input.Request);
    }
}
