using MediatR;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.Requests;

/// <summary>
/// This request simulates retrieving the status of the failing request, which has a change of actually doing something even though it reports an error.
/// If the request actually applied, this request will return a response with an Id in it.
/// </summary>
/// <param name="Id"></param>
public record CheckIfFailingRequestAppliedRequest(Guid Id) : IRequest<BasicResponse?>;
