using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.ThirdParties.Dtos;

public sealed record ThirdPartyDetailsDto(string Name, IEnumerable<Address>? Addresses = null, int Key = 0, 
CustomerRole? Customer = null, SupplierRole? Supplier = null) : ThirdPartyDto(Key, Name);