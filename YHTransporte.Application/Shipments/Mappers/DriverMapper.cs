using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.Mappers;

internal sealed class DriverMapper : IMapper<DriverDto, Driver>
{
    public static Driver ToEntity(DriverDto value)
    => new(value.Name, value.MiddleName, value.LastName, value.SecondLastName)
    { InUse = value.InUse, Code = value.Code };

    public static DriverDto ToValue(Driver entity)
    => new(entity.Code, entity.Name, entity.MiddleName, entity.LastName, entity.SecondLastName, entity.InUse);
}