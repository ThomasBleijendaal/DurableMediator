using DurableMediator.OutOfProcess;
using MediatR;
using Moq;

namespace DurableMediator.OutOfProcess.Testing;

internal class ScenarioSetup<TWorkflowRequest> : IScenarioSetup
{
    private readonly Mock<IWorkflowExecution<TWorkflowRequest>> _mock;

    public ScenarioSetup(Mock<IWorkflowExecution<TWorkflowRequest>> mock)
    {
        _mock = mock;
    }

    public void SetupRequest<TRequest, TResponse>(TResponse response)
        where TRequest : IRequest<TResponse>
    {
        _mock.Setup(x => x.SendAsync(
                It.IsAny<TRequest>()))
            .ReturnsAsync(response);
    }

    public void SetupRequestThrows<TRequest, TResponse>(Exception exception)
        where TRequest : IRequest<TResponse>
    {
        _mock.Setup(x => x.SendAsync(
                It.IsAny<TRequest>()))
            .ThrowsAsync(exception);
    }

    public void SetupRequestWithCheck<TRequest, TCheck, TResponse>(TResponse response)
        where TRequest : IRequest<TResponse>
        where TCheck : IRequest<TResponse?>
    {
        _mock.Setup(x => x.SendWithCheckAsync(
                It.IsAny<TRequest>(),
                It.IsAny<TCheck>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(response);
    }

    public void SetupRequestWithCheckThrows<TRequest, TCheck, TResponse>(Exception exception)
        where TRequest : IRequest<TResponse>
        where TCheck : IRequest<TResponse?>
    {
        _mock.Setup(x => x.SendWithCheckAsync(
                It.IsAny<TRequest>(),
                It.IsAny<TCheck>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan?>()))
            .ThrowsAsync(exception);
    }

    public void SetupCallSubWorkflow<TSubWorkflowRequest, TWorkflowResponse>(TWorkflowResponse response)
        where TSubWorkflowRequest : IWorkflowRequest<TWorkflowResponse>
    {
        _mock.Setup(x => x.CallSubWorkflowAsync(
                    It.IsAny<TSubWorkflowRequest>()))
            .ReturnsAsync(response);
    }

}
