using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.Mappers;

internal static class AddressMappers
{
    public static AddressDetailsDto AddressToDetailedDto(Address address)
    => new(address.Key, address.Details, MunicipalityToDto(address.Municipality));

    public static MunicipalityDto MunicipalityToDto(Municipality municipality)
    => new(municipality.Key, municipality.Name, DepartmentToDto(municipality.Department));

    public static DepartmentDto DepartmentToDto(Department department)
    => new(department.Key, department.Name);

    //Tape
    public static Address AddressKeyToAddress(AddressKey command)
    => new(command.Name, new Municipality("-", new("-")){Key = command.PlaceId});

    public static Municipality AddressKeyToMunicipality(AddressKey command)
    =>new(command.Name, new Department("-"){Key = command.PlaceId});

}