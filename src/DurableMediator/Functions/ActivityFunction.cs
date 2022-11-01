namespace DurableMediator.Functions;

internal static class ActivityFunction
{
    public const string SendObject = "Request";
    public const string SendObjectWithResponse = "RequestWithResponse";
    public const string SendObjectWithCheckAndResponse = "RequestWithResponseCheck";

    public static Task SendObjectAsync(
        MediatorRequest request, IActivityExecutor executor)
        => executor.SendObjectAsync(request);

    public static Task<MediatorResponse> SendObjectWithResponseAsync(
        MediatorRequestWithResponse request, IActivityExecutor executor)
        => executor.SendObjectWithResponseAsync(request);

    public static Task<MediatorResponse> SendObjectWithCheckAndResponseAsync(
        MediatorRequestWithCheckAndResponse request, IActivityExecutor executor)
        => executor.SendObjectWithCheckAndResponseAsync(request);
}
