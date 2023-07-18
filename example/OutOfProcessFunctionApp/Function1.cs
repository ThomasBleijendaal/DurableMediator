using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace OutOfProcessFunctionApp;

/*
 *  TODOs:
 * V check if workflow is still picked up once it is inherited
 * - check if workflow can still access activities when its in a nuget package
 * - implement all json converters for all models
 * - add IWorkflowRequest<> to Workflow
 * - update MediatR
 * - ConfigureAwait
 * - all the execution specials
 * - merge stuff into abstractions package
 */

[DurableTask(nameof(DurableMediator))]
public class DurableMediator : TaskActivity<MediatorRequest, Unit>
{
    private readonly IMediator _mediator;

    public DurableMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Unit> RunAsync(TaskActivityContext context, MediatorRequest input)
    {
        return await _mediator.Send(input.Request);
    }
}

[DurableTask(nameof(DurableMediatorWithResponse))]
public class DurableMediatorWithResponse : TaskActivity<MediatorRequestWithResponse, MediatorResponse>
{
    private readonly IMediator _mediator;

    public DurableMediatorWithResponse(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<MediatorResponse> RunAsync(TaskActivityContext context, MediatorRequestWithResponse input)
    {
        return new MediatorResponse(await _mediator.Send(input.Request));
    }
}

public abstract class Workflow<TRequest, TResponse> : TaskOrchestrator<TRequest, TResponse>
{
    public override sealed async Task<TResponse> RunAsync(TaskOrchestrationContext context, TRequest input)
    {
        var executor = new OrchestratorExecutor<TRequest, TResponse>(context, input, context.CreateReplaySafeLogger(GetType()));
        return await OrchestrateAsync(executor);
    }

    public abstract Task<TResponse> OrchestrateAsync(IWorkflowExecution<TRequest> execution);
}

public interface IWorkflowExecution<TRequest>
{
    TaskOrchestrationContext OrchestrationContext { get; }

    /// <summary>
    /// A replay safe logger for use during the execution of the workflow.
    /// </summary>
    ILogger ReplaySafeLogger { get; }

    TRequest Request { get; }

    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);
}

public class OrchestratorExecutor<TRequest, TResponse> : IWorkflowExecution<TRequest>
{
    private readonly TaskOrchestrationContext _context;
    private readonly TRequest _request;
    private readonly ILogger _logger;

    public OrchestratorExecutor(TaskOrchestrationContext context, TRequest request, ILogger logger)
    {
        _context = context;
        _request = request;
        _logger = logger;
    }

    public TaskOrchestrationContext OrchestrationContext => _context;

    public ILogger ReplaySafeLogger => _logger;

    public TRequest Request => _request;

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        if (typeof(TResponse) == typeof(Unit))
        {
            await _context.CallDurableMediatorAsync(new MediatorRequest((IRequest<Unit>)request));

            return default!;
        }

        var response = await _context.CallDurableMediatorWithResponseAsync(new MediatorRequestWithResponse(request));

        if (response == null)
        {
            throw new InvalidOperationException("Received an empty response");
        }

        return ((JsonElement)response.Response).Deserialize<TResponse>()
            ?? throw new InvalidOperationException("Cannot deserialize response");
    }
}

[JsonConverter(typeof(MediatorRequestJsonConverter))]
public record MediatorRequest(IRequest<Unit> Request)
{
    public string RequestType => Request.GetType().AssemblyQualifiedName ?? Request.GetType().Name;
}

[JsonConverter(typeof(MediatorRequestJsonConverter))]
public record MediatorRequestWithResponse(dynamic Request);

public record MediatorRequestWithResponseJsonModel(dynamic Request, string Type);

public record MediatorResponse(object Response);

public class MediatorRequestJsonConverter : JsonConverter<MediatorRequestWithResponse>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(MediatorRequestWithResponse);

    public override MediatorRequestWithResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var wrapper = JsonSerializer.Deserialize<MediatorRequestWithResponseJsonModel>(ref reader, options);

        if (wrapper?.Type != null)
        {
            var type = Type.GetType(wrapper.Type);

            if (type != null)
            {
                var request = ((JsonElement)wrapper.Request).Deserialize(type, options);

                if (request != null)
                {
                    return new MediatorRequestWithResponse(request);
                }
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, MediatorRequestWithResponse value, JsonSerializerOptions options)
    {
        var requestObject = value.Request as object;

        JsonSerializer.Serialize(writer, new MediatorRequestWithResponseJsonModel(value.Request, requestObject.GetType().AssemblyQualifiedName), options);
    }
}

