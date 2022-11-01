using MediatR;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.Requests;

/// <summary>
/// The slow requests simulates a low API response.
/// </summary>
/// <param name="Id"></param>
internal record SlowRequest(Guid Id) : IRequest<BasicResponse>;
