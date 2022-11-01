using MediatR;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.Requests;

/// <summary>
/// This request simulates an API that has a high change of failing, and has to be retried multiple times.
/// </summary>
/// <param name="Id"></param>
internal record ErrorProneRequest(Guid Id) : IRequest<SuccessResponse>;
