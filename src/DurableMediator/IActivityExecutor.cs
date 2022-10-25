﻿namespace DurableMediator;

internal interface IActivityExecutor
{
    Task SendObjectAsync(MediatorRequest request);
    Task<MediatorResponse> SendObjectWithResponseAsync(MediatorRequestWithResponse request);
}
