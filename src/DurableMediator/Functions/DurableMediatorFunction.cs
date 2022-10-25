namespace DurableMediator.Functions;

internal static class ActivityFunction
{
    public const string SendObject = "MediatorCommand";
    public const string SendObjectWithResponse = "MediatorQuery";

    public static Task SendObjectAsync(
        MediatorRequest request, IActivityExecutor executor)
        => executor.SendObjectAsync(request);
    
    public static Task<MediatorResponse> SendObjectWithResponseAsync(
        MediatorRequestWithResponse request, IActivityExecutor executor)
        => executor.SendObjectWithResponseAsync(request);
}
