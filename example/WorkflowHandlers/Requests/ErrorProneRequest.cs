using MediatR;
using WorkflowHandlers.Responses;

namespace WorkflowHandlers.Requests;

/// <summary>
/// This request simulates an API that has a high change of failing, and has to be retried multiple times.
/// </summary>
/// <param name="Id"></param>
public record ErrorProneRequest(Guid Id) : IRequest<SuccessResponse>;
