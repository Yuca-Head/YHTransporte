using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.ThirdParties.Results;

public record ThirdPartyNotFound(int Key = -1) : NotFound(Key);