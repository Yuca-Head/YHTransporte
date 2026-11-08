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
params IEnumerable<PackageDto> Packages)
{
    // CAMBIO: totales para las listas del editor de órdenes/envíos.
    public int PackageCount => Packages.Count();

    public decimal TotalWeight => Packages.Sum(x => x.Weight);

    public bool IsPending => Status is ShipmentStatuses.Pending;
}
