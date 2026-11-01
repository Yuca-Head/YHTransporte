using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Shipments.Results;

public sealed record DriverNotFound(string Code) : NotFound(Code);