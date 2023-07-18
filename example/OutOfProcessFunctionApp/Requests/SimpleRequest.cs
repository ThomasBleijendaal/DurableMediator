using MediatR;
using OutOfProcessFunctionApp.Responses;

namespace OutOfProcessFunctionApp.Requests;

/// <summary>
/// A simple requests just returns a basic response with an Id in it.
/// </summary>
/// <param name="Id"></param>
internal record SimpleRequest(Guid Id, string Description) : IRequest<BasicResponse>;
