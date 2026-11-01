using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.Mappers;

internal sealed class ShipmentMapper : IMapper<ShipmentDto, Shipment>
{
    public static Shipment ToEntity(ShipmentDto value)
    => Shipment.Restore(value.Key, value.OrderKey, value.Status, value.Driver is null? null : DriverMapper.ToEntity(value.Driver),
    value.Vehicle is null ? null : VehicleMapper.ToEntity(value.Vehicle), value.Description, value.EstimatedPickUpAt, value.PickedUpAt,
    value.DeliveredAt, value.Packages.Select(PackageMapper.ToEntity));

    public static ShipmentDto ToValue(Shipment entity)
    => new(entity.Key, entity.OrderKey, entity.Status, entity.Driver is null? null : DriverMapper.ToValue(entity.Driver),
    entity.Vehicle is null? null : VehicleMapper.ToValue(entity.Vehicle), entity.Description, entity.EstimatedPickUpAt,
    entity.PickedUpAt, entity.DeliveredAt, entity.Packages.Select(PackageMapper.ToValue));
}