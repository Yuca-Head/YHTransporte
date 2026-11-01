using YHTransporte.Application.Addresses.Dtos;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Orders.Dtos;

public record OrderDto(SupplierRoleDto Supplier, CustomerRoleDto Customer, AddressDetailsDto Origin, AddressDetailsDto Destination, 
OrderStatuses Status = OrderStatuses.Pending);