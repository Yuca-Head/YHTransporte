using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Addresses.Results;

public sealed record AddressNotFound (params IEnumerable<int> Ids) : NotFound(Ids);