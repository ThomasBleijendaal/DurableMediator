using MediatR;

namespace WorkflowFunctionApp.Requests;

internal record RequestB(Guid Id) : IRequest<RequestBResponse>;

