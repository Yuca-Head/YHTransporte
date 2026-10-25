using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Addresses.Results;

public sealed record DepartmentNotFound (IEnumerable<int> Ids) : NotFound(Ids);