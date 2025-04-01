namespace DurableMediator.HostedService.Models;

internal record MediatorRequestWithCheckAndResponseJsonModel(dynamic Request, string Type, dynamic CheckIfRequestApplied, string CheckType);
