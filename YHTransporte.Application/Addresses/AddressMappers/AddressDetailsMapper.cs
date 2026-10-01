using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.AddressMappers;

public class AddressDetailsMapper : IMapper<AddressDetailsDto, Address>
{

    public static Address ToEntity(AddressDetailsDto value)
    => new(value.Name, MunicipalityMapper.ToEntity(value.Municipality)) {Key = value.Id};


    public static AddressDetailsDto ToValue(Address address)
     => new(address.Key, address.Details, MunicipalityMapper.ToValue(address.Municipality));
}