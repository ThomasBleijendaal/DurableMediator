using MediatR;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.Requests;

/// <summary>
/// A simple requests just returns a basic response with an Id in it.
/// </summary>
/// <param name="Id"></param>
public record SimpleRequest(Guid Id, string Description) : IRequest<BasicResponse>;
