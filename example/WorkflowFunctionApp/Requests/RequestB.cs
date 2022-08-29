using DurableMediator;
using MediatR;

namespace WorkflowFunctionApp.Requests;

internal record RequestB(Guid Id) : IRequest<RequestBResponse>;

internal record RequestBResponse(bool IsSuccess, Guid Id) : IRetryResponse;

