using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Mappers;

public sealed class ThirdPartyAddressSqlMapper : IMapper<ThirdPartyAddressSqlDto, Address>
{
    public static Address ToEntity(ThirdPartyAddressSqlDto value)
    => new(value.Details,
    new Municipality(value.MunicipalityName,
    new Department(value.DepartmentName) { Key = value.DepartmentId }) //Department
    { Key = value.IdMunicipality } //Municipality Id
    )
    { Key = value.Id }; //Address Id

    // El Dto de third party necesita el dueño (IdThirdParty), que la entidad Address no tiene.
    // Por eso aquí se deja en 0; el repositorio es quien conoce el dueño.
    public static ThirdPartyAddressSqlDto ToValue(Address entity)
    => new(0, entity.Key, entity.Details,
        entity.Municipality.Key, entity.Municipality.Name,
        entity.Municipality.Department.Key, entity.Municipality.Department.Name);
}