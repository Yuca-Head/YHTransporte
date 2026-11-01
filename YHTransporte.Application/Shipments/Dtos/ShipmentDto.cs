using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Shipments.Dtos;

public sealed record ShipmentDto(      
string Key,
string OrderKey,
ShipmentStatuses Status,
DriverDto? Driver,
VehicleDto? Vehicle,
string? Description,
DateTimeOffset? EstimatedPickUpAt,
DateTimeOffset? PickedUpAt,
DateTimeOffset? DeliveredAt,
params IEnumerable<PackageDto> Packages);