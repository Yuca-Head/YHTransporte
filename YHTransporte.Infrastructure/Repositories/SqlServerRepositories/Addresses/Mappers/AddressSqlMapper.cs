using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Mappers;

public sealed class AddressSqlMapper : IMapper<AddressSqlDto, Address>
{
    public static Address ToEntity(AddressSqlDto value)
    => new(value.Details, 
    new Municipality(value.MunicipalityName, 
    new Department(value.DepartmentName){Key = value.DepartmentId}) //Department Id
    {Key = value.MunicipalityId} //Municipality Id
    ){Key = value.Id}; //Address Id

    public static AddressSqlDto ToValue(Address entity)
    {
        Municipality municipality = entity.Municipality;
        Department department = municipality.Department;

        return new(entity.Key, entity.Details, municipality.Key, municipality.Name, department.Key, department.Name);
    }
}