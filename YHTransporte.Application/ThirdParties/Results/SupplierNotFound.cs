namespace YHTransporte.Application.ThirdParties.Results;

public sealed record SupplierNotFound(int Key) : ThirdPartyNotFound(Key);