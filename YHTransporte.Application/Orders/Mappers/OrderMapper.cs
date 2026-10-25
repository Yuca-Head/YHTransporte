using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.AddressMappers;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.ThirdParties.Mappers;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Orders.Mappers;

internal sealed class OrderMapper : IMapper<OrderDto, Order>
{
    public static Order ToEntity(OrderDto value)
    => new(SupplierRoleMapper.ToEntity(value.Supplier), CustomerRoleMapper.ToEntity(value.Customer),
    AddressDetailsMapper.ToEntity(value.Origin), AddressDetailsMapper.ToEntity(value.Destination));

    public static OrderDto ToValue(Order entity)
    => new(SupplierRoleMapper.ToValue(entity.Supplier), CustomerRoleMapper.ToValue(entity.Customer),
    AddressDetailsMapper.ToValue(entity.OriginDirection), AddressDetailsMapper.ToValue(entity.DestinationDirection));
    
}