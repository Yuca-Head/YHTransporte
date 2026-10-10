using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.AddressMappers;


public static class AddressKeyMappers
{
    public class MunicipalityMapper : IMapper<AddressKey, Municipality>
    {
        public static Municipality ToEntity(AddressKey value)
        => new(value.Name, new("-"){Key = value.PlaceId}); 

        public static AddressKey ToValue(Municipality entity)
        => new(entity.Name, entity.Department.Key);
    }

    public class AddressMapper : IMapper<AddressKey, Address>
    {
        public static Address ToEntity(AddressKey value)
        => new(value.Name, new("-", new("-")){Key = value.PlaceId});
        public static AddressKey ToValue(Address entity)
        => new(entity.Details, entity.Municipality.Key);
    }
}