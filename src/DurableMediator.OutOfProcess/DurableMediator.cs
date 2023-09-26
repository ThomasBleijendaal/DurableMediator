namespace DurableMediator.OutOfProcess;

/*
 *  TODOs:
 * V check if workflow is still picked up once it is inherited
 * X check if workflow can still access activities when its in a nuget package -> NOPE
 * v implement all json converters for all models
 * v add IWorkflowRequest<> to IWorkflowExecution
 * - update MediatR
 * v ConfigureAwait
 * v all the execution specials
 * - merge stuff into abstractions package
 * - make dependency collection extension
 * - add tracing
 * v remove generated extensions and refactor
 * v internalize everything that needs to be internal (also look at example projects)
 * - add mediator requests to history
 */

//public class DurableMediator : TaskActivity<MediatorRequest, Unit>
//{
//    private readonly IMediator _mediator;

//    public DurableMediator(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    public override async Task<Unit> RunAsync(TaskActivityContext context, MediatorRequest input)
//    {
//        return await _mediator.Send(input.Request);
//    }
//}
