using OneOf.Types;
using YHTransporte.Application.Addresses.Results;
using YHTransporte.Application.ThirdParties.Results;
using OneOf;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Core.Entities;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.ThirdParties.Mappers;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdPartyAddress;

public sealed class CreateThirdPartyAddressHandler(IThirdPartyRepository thirdPartyRepository, IAddressRepository addressRepository)
{
    private readonly IThirdPartyRepository _tpRepository = thirdPartyRepository;
    private readonly IAddressRepository _addressRepository = addressRepository;
    public async Task<OneOf<Success, ThirdPartyNotFound, AddressNotFound, 
    AlreadyContainsItem<ThirdPartyDto, int>>> Handle(CreateThirdPartyAddressCommand command, CancellationToken cancellation = default)
    {
        var thirdParty = (await _tpRepository.GetByKeysAsync([command.ThirdPartyId], cancellation)).FirstOrDefault();    
        int addressId = command.AddressId;  

        if(thirdParty is null)
            return new ThirdPartyNotFound(command.ThirdPartyId);

        if(thirdParty.Addresses.Any(x => x.Key == addressId))
            return new AlreadyContainsItem<ThirdPartyDto, int>
            (ThirdPartyDatilsMapper.ToValue(thirdParty), addressId);

        if(!await _addressRepository.Exists(addressId, cancellation))
            return new AddressNotFound(addressId);
        
        await _tpRepository.AddAddressToThirdParty(command.AddressId, command.ThirdPartyId, cancellation);
        return new Success();
    }
}