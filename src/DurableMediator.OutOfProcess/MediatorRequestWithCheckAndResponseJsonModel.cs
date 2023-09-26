namespace DurableMediator.OutOfProcess;

internal record MediatorRequestWithCheckAndResponseJsonModel(dynamic Request, string Type, dynamic CheckIfRequestApplied, string CheckType);
