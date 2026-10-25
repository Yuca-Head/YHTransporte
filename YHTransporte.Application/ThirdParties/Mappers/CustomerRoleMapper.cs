using YHTransporte.Application.Abstractions;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Mappers;

internal sealed class CustomerRoleMapper : IMapper<CustomerRoleDto, CustomerRole>
{
    public static CustomerRole ToEntity(CustomerRoleDto value)
    => new(value.ThirdPartyKey);

    public static CustomerRoleDto ToValue(CustomerRole entity)
    => new(entity.ThirdPartyKey);
}