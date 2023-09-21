using DurableMediator.OutOfProcess;
using MediatR;

namespace OutOfProcessFunctionApp.Workflows;

public record BasicWorkflowRequest(Guid RequestId) : IWorkflowRequest<Unit>
{
    public static Type Workflow => typeof(BasicWorkflow);

    public string InstanceId => $"basic-{RequestId}";
}
