using MediatR;

namespace WorkflowHandlers.Requests;

/// <summary>
/// A basic requests that does not return any data.
/// </summary>
/// <param name="Id"></param>
public record CommandRequest(Guid Id, string Description) : IRequest<Unit>;
