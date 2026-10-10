using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.ThirdParties.Dtos;

public sealed record ThirdPartyDetailsDto(string Name, int Key = 0, 
CustomerRole? Customer = null, SupplierRole? Supplier = null) : ThirdPartyDto(Key, Name)
{
    public IList<AddressDetailsDto> Addresses {get; internal set;} = [];

    public bool IsCustomer => Customer != null;
    public bool IsSupplier => Supplier != null;
}