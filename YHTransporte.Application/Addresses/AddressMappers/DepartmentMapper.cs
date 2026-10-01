using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.AddressMappers;

public class DepartmentMapper : IMapper<DepartmentDto, Department>
{
    public static Department ToEntity(DepartmentDto value)
    => new(value.Name) {Key = value.Id};

    public static DepartmentDto ToValue(Department entity)
    => new(entity.Key, entity.Name);
}