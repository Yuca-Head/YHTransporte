using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.Mappers;

internal sealed class PackageMapper : IMapper<PackageDto, Package>
{
    public static Package ToEntity(PackageDto value)
    => new(ProductMapper.ToEntity(value.Product), value.Weight, value.ProductAmount)
    {Key = value.Key, ShipmentCode = value.ShipmentCode};

    public static PackageDto ToValue(Package entity)
    => new(entity.Key, ProductMapper.ToValue(entity.Product), entity.Weight, entity.ProductAmount, entity.ShipmentCode);
}