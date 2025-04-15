using MediatR;

namespace DurableMediator.HostedService.Testing;

public interface IScenarioSetup
{
    void SetupRequest<TRequest, TResponse>(TResponse response)
        where TRequest : IRequest<TResponse>;

    void SetupRequestThrows<TRequest, TResponse>(Exception exception)
        where TRequest : IRequest<TResponse>;

    void SetupRequestWithCheck<TRequest, TCheck, TResponse>(TResponse response)
        where TRequest : IRequest<TResponse>
        where TCheck : IRequest<TResponse?>;

    void SetupRequestWithCheckThrows<TRequest, TCheck, TResponse>(Exception exception)
        where TRequest : IRequest<TResponse>
        where TCheck : IRequest<TResponse?>;

    void SetupCallSubWorkflow<TWorkflowRequest, TWorkflowResponse>(TWorkflowResponse response)
        where TWorkflowRequest : IWorkflowRequest<TWorkflowResponse>;
}
