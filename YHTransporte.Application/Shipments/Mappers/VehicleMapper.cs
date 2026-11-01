using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.Mappers;

internal sealed class VehicleMapper : IMapper<VehicleDto, Vehicle>
{
    public static Vehicle ToEntity(VehicleDto value)
    => new(){ Key = value.Id, Plate = value.Plate, Description = value.Description, InUse = value.InUse};

    public static VehicleDto ToValue(Vehicle entity)
    => new(entity.Key, entity.Plate, entity.Description, entity.InUse);
}