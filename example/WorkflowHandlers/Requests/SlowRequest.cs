using MediatR;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.Requests;

/// <summary>
/// The slow requests simulates a low API response.
/// </summary>
/// <param name="Id"></param>
public record SlowRequest(Guid Id) : IRequest<BasicResponse>;
