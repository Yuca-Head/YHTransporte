using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Mappers;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Mappers;

internal sealed class SupplierRoleMapper : IMapper<SupplierRoleDto, SupplierRole>
{
    public static SupplierRole ToEntity(SupplierRoleDto value)
    => new (value.ThirdPartyKey, value.Products.Select(ProductMapper.ToEntity));


    public static SupplierRoleDto ToValue(SupplierRole entity)
    => new(entity.ThirdPartyKey, entity.Products.Select(ProductMapper.ToValue));
}