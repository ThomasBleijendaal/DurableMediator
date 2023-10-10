using DurableMediator.OutOfProcess;
using MediatR;

namespace OutOfProcessFunctionApp.Package.Workflows;

public record BasicWorkflowRequest(Guid RequestId) : IWorkflowRequest<Unit>
{
    public string WorkflowName => nameof(BasicWorkflow);

    public string InstanceId => $"basic-{RequestId}";
}
