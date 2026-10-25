using YHTransporte.Application.Addresses.Dtos;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Orders.Dtos;

public record OrderDto(SupplierRoleDto Supplier, CustomerRoleDto Customer, AddressDetailsDto Origin, AddressDetailsDto Destination);