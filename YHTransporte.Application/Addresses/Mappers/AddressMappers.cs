using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.Mappers;

public static class AddressMapper
{
    public static AddressDetailsDto AddressToDetailedDto(Address address)
    => new(address.Key, MunicipalityToDto(address.Municipality));

    public static MunicipalityDto MunicipalityToDto(Municipality municipality)
    => new(municipality.Key, DepartmentToDto(municipality.Department));

    public static DepartmentDto DepartmentToDto(Department department)
    => new(department.Key, department.Name);
}