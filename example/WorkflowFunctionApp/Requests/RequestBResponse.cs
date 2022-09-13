using DurableMediator;

namespace WorkflowFunctionApp.Requests;

internal record RequestBResponse(bool IsSuccess, Guid Id) : IRetryResponse;

