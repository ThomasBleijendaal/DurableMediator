//using MediatR;
//using Microsoft.DurableTask;

//namespace DurableMediator.OutOfProcess;

//public class DurableMediatorWithResponse : TaskActivity<MediatorRequestWithResponse, MediatorResponse>
//{
//    private readonly IMediator _mediator;

//    public DurableMediatorWithResponse(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    public override async Task<MediatorResponse> RunAsync(TaskActivityContext context, MediatorRequestWithResponse input)
//    {
//        return new MediatorResponse(await _mediator.Send(input.Request));
//    }
//}
