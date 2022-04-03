using MediatR;

namespace WorkflowFunctionApp.Requests;

internal record RequestB(Guid Id) : IRequest<RequestBResponse>;

internal record RequestBResponse(bool Success);

