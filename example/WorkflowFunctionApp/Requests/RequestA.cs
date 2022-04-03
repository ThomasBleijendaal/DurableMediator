using MediatR;

namespace WorkflowFunctionApp.Requests;

internal record RequestA(Guid Id) : IRequest<RequestAResponse>;

internal record RequestAResponse(Guid Id);

