namespace WorkflowFunctionApp.Workflows;

public record BasicWorkflowRequest(Guid RequestId)
{
    public string WorkflowName => "Basic";

    public string InstanceId => $"basic-{RequestId}";
}
