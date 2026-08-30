using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Mappers;

public static class ThirdPartyMappers
{
    public static ThirdParty ToThirdParty(ThirdPartyDetailsDto dto)
    {
        ThirdParty result = new(dto.Name){ Key = dto.Key};

        result.AddAddresses(dto.Addresses ?? []);
        
        if(dto.Customer is not null)
            result.BecomeCustomer();
        if(dto.Supplier is not null)
            result.BecomeSupplier();
        
        return result;
        
    }

    public static ThirdPartyDetailsDto ToDetailedDto(ThirdParty entity)
    => new(entity.Name, entity.Addresses, entity.Key, entity.Customer, entity.Supplier);

    public static ThirdPartyDto ToDto(ThirdParty entity)
    => new(entity.Key, entity.Name);
}