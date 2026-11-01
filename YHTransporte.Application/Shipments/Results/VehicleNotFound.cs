using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Shipments.Results;

public sealed record VehicleNotFound(int Id) : NotFound(Id);