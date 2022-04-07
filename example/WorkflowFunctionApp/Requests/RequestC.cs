using MediatR;

namespace WorkflowFunctionApp.Requests;

internal record RequestC(Guid Id) : IRequest;

