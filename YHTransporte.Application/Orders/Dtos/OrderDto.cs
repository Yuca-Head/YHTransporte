using YHTransporte.Application.Addresses.Dtos;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Orders.Dtos;

public record OrderDto(SupplierRoleDto Supplier, CustomerRoleDto Customer, AddressDetailsDto Origin, AddressDetailsDto Destination,
OrderStatuses Status = OrderStatuses.Pending)
{
    //I'd really forgotten about this.
    public string Code{get; init;} = "";

    // CAMBIO: el editor de órdenes necesita mostrar la fecha de creación y la descripción.
    public DateTimeOffset CreatedAt {get; init;}

    public string Description {get; init;} = "";

    public bool IsDone => Status is OrderStatuses.Completed or OrderStatuses.Canceled;
};