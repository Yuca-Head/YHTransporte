namespace YHTransporte.Application.Shipments.Dtos;

public sealed record VehicleDto(int Id, string Plate, string Description, bool InUse);