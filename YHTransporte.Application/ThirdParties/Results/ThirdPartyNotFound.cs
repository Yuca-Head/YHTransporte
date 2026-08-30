using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.ThirdParties.Results;

public sealed record ThirdPartyNotFound(int Key = -1) : NotFound(Key);