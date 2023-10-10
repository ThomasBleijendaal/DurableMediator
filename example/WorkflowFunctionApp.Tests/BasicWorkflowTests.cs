using DurableMediator;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WorkflowFunctionApp.Workflows;
using WorkflowHandlers.Requests;
using WorkflowHandlers.Responses;

namespace WorkflowFunctionApp.Tests;

public class BasicWorkflowTests
{
    private BasicWorkflow _subject;
    private Mock<IWorkflowExecution<BasicWorkflowRequest>> _executionMock;
    private readonly Guid _requestId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _subject = new BasicWorkflow();

        _executionMock = new Mock<IWorkflowExecution<BasicWorkflowRequest>>(MockBehavior.Strict);

        _executionMock.SetupGet(x => x.ReplaySafeLogger).Returns(NullLogger.Instance);
        _executionMock.SetupGet(x => x.Request).Returns(new BasicWorkflowRequest(_requestId));
        _executionMock.SetupGet(x => x.OrchestrationContext).Returns(new Mock<IDurableOrchestrationContext>().Object);

        var sequence = new MockSequence();

        SetupSimpleRequest(sequence, "1");
        SetupSimpleRequest(sequence, "2");
        SetupSimpleRequest(sequence, "3");

        SetupSimpleRequest(sequence, "A");
        SetupSimpleRequest(sequence, "B");
        SetupSimpleRequest(sequence, "C");
        SetupSimpleRequest(sequence, "D");
        SetupSimpleRequest(sequence, "E");
        SetupSimpleRequest(sequence, "F");
        SetupSimpleRequest(sequence, "G");
        SetupSimpleRequest(sequence, "H");
        SetupSimpleRequest(sequence, "I");
        SetupSimpleRequest(sequence, "J");
        SetupSimpleRequest(sequence, "K");
        SetupSimpleRequest(sequence, "L");
        SetupSimpleRequest(sequence, "M");
        SetupSimpleRequest(sequence, "N");
        SetupSimpleRequest(sequence, "O");
        SetupSimpleRequest(sequence, "P");
        SetupSimpleRequest(sequence, "Q");
        SetupSimpleRequest(sequence, "R");
        SetupSimpleRequest(sequence, "S");
        SetupSimpleRequest(sequence, "T");
        SetupSimpleRequest(sequence, "U");
        SetupSimpleRequest(sequence, "V");
        SetupSimpleRequest(sequence, "W");
        SetupSimpleRequest(sequence, "X");
        SetupSimpleRequest(sequence, "Y");
        SetupSimpleRequest(sequence, "Z");

        SetupSlowRequest(sequence);

        _executionMock
            .InSequence(sequence)
            .Setup(x => x.CallSubWorkflowAsync(
                It.IsAny<ReusableWorkflowRequest>()))
            .ReturnsAsync(new ReusableWorkflowResponse(""));

        _executionMock
            .InSequence(sequence)
            .Setup(x => x.StartWorkflow(
                It.IsAny<ReusableWorkflowRequest>()));
    }

    private void SetupSimpleRequest(MockSequence sequence, string expectedDescription)
    {
        _executionMock
            .InSequence(sequence)
            .Setup(x => x.SendAsync(
                It.Is<SimpleRequest>(request => request.Id == _requestId && request.Description == expectedDescription)))
            .ReturnsAsync(new BasicResponse(Guid.NewGuid()));
    }

    private void SetupSlowRequest(MockSequence sequence)
    {
        _executionMock
            .InSequence(sequence)
            .Setup(x => x.SendAsync(
                It.Is<SlowRequest>(request => request.Id == _requestId)))
            .ReturnsAsync(new BasicResponse(Guid.NewGuid()));
    }

    [Test]
    public async Task WorkflowRunTestAsync()
    {
        await _subject.OrchestrateAsync(_executionMock.Object);

        _executionMock.VerifyAll();
    }
}
