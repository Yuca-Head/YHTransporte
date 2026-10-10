using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Mappers;

public sealed class DepartmentSqlMapper : IMapper<DepartmentSqlDto, Department>
{
    public static Department ToEntity(DepartmentSqlDto value)
    => new(value.Name) { Key = value.Id };

    public static DepartmentSqlDto ToValue(Department entity)
    => new(entity.Key, entity.Name);
}