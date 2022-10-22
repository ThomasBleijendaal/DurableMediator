using DurableMediator;

namespace WorkflowFunctionApp.Responses;

internal record SuccessResponse(Guid Id, bool IsSuccess) : IRetryResponse;


