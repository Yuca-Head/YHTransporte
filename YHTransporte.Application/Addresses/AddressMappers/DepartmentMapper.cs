using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.AddressMappers;

internal class DepartmentMapper : IMapper<DepartmentDto, Department>
{
    public static Department ToEntity(DepartmentDto value)
    => new(value.Name) {Key = value.Id};

    public static DepartmentDto ToValue(Department entity)
    => new(entity.Key, entity.Name);
}