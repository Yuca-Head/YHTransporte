namespace YHTransporte.Application.ThirdParties.Results;

public sealed record CustomerNotFound(int Key) : ThirdPartyNotFound(Key);