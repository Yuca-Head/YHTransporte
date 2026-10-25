using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Mappers;

public sealed class MunicipalitySqlMapper : IMapper<MunicipalitySqlDto, Municipality>
{
    public static Municipality ToEntity(MunicipalitySqlDto value)
    =>new(value.Name, 
    new Department(value.DepartmentName){Key = value.DepartmentId}) //Department
    {Key = value.Id};

    public static MunicipalitySqlDto ToValue(Municipality entity)
    =>new(entity.Key, entity.Name, entity.Department.Key, entity.Department.Name);
}