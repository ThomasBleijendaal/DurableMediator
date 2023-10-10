using MediatR;

namespace WorkflowHandlers.Requests;

/// <summary>
/// A basic requests that does not return any data.
/// </summary>
/// <param name="Id"></param>
public record CommandRequest(Guid Id, string Description) :
#if NET7_0_OR_GREATER
    IRequest;
#else
    IRequest<Unit>;
#endif
