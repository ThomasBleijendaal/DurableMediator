﻿using DurableMediator;
using WorkflowFunctionApp.Responses;

namespace WorkflowFunctionApp.Workflows;

internal record ReusableWorkflowRequest(Guid SomeId) : IWorkflowRequest<ReusableWorkflowResponse>
{
    public string WorkflowName => "Reusable";

    public string InstanceId => $"reusable-{SomeId}";
}
