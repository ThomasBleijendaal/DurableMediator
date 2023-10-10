using MediatR;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.Requests;

/// <summary>
/// A failing request simulates an API that returns errors, but has a change of actually applying the request while returning an error.
/// This simulates a timeout error, while the web server is still processing the request anyway.
/// </summary>
/// <param name="Id"></param>
public record FailingRequest(Guid Id) : IRequest<BasicResponse>;
