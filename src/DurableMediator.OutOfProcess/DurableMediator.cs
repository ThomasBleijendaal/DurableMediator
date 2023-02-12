namespace DurableMediator.OutOfProcess;

/*
 *  TODOs:
 * V check if workflow is still picked up once it is inherited
 * X check if workflow can still access activities when its in a nuget package -> NOPE
 * v implement all json converters for all models
 * v add IWorkflowRequest<> to IWorkflowExecution
 * v update MediatR
 * v ConfigureAwait
 * v all the execution specials
 * v merge stuff into abstractions package
 * v make dependency collection extension
 * v add tracing
 * v remove generated extensions and refactor
 * v internalize everything that needs to be internal (also look at example projects)
 * - STRETCH: scenario testing
 * - STRETCH: add mediator requests to history
 */
