using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.AddressMappers;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Mappers;

public sealed class ThirdPartyDatilsMapper : IMapper<ThirdPartyDetailsDto, ThirdParty>
{


    public static ThirdPartyDetailsDto ToValue(ThirdParty entity)
    => new(entity.Name, entity.Key, entity.Customer, entity.Supplier);

    public static ThirdParty ToEntity(ThirdPartyDetailsDto dto)
    {
        ThirdParty result = new(dto.Name){ Key = dto.Key};

        result.AddAddresses(dto.Addresses.Select(AddressDetailsMapper.ToEntity));
        
        if(dto.Customer is not null)
            result.BecomeCustomer();
        if(dto.Supplier is not null)
            result.BecomeSupplier();
        
        return result;
        
    }
}