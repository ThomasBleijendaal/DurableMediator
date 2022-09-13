using MediatR;

namespace WorkflowFunctionApp.Requests;

internal record RequestA(Guid Id) : IRequest<RequestAResponse>;

