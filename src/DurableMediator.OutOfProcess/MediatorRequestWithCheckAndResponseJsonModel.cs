namespace DurableMediator.OutOfProcess;

public record MediatorRequestWithCheckAndResponseJsonModel(dynamic Request, string Type, dynamic CheckIfRequestApplied, string CheckType);
