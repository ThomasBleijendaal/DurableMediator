namespace DurableMediator.Functions;

internal static class ActivityFunction
{
    public const string SendObject = "Request";
    public const string SendObjectWithResponse = "RequestWithResponse";
    public const string SendObjectWithCheckAndResponse = "RequestWithResponseCheck";

    public static Task SendObjectAsync(
        MediatorRequest request, IDurableMediator mediator)
        => mediator.SendObjectAsync(request);

    public static Task<MediatorResponse> SendObjectWithResponseAsync(
        MediatorRequestWithResponse request, IDurableMediator mediator)
        => mediator.SendObjectWithResponseAsync(request);

    public static Task<MediatorResponse> SendObjectWithCheckAndResponseAsync(
        MediatorRequestWithCheckAndResponse request, IDurableMediator mediator)
        => mediator.SendObjectWithCheckAndResponseAsync(request);
}
