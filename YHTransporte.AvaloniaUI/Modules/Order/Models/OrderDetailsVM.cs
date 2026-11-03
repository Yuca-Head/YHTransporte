using System.Threading.Tasks;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Shared.Contexts;

namespace YHTransporte.AvaloniaUI.Modules.Order.Models;

public readonly record struct OrderDetailsVM(OrderDto Dto)
{
    public readonly ThirdPartyDto? Customer {get; init;} = null;
    public readonly ThirdPartyDto? Supplier {get; init;} = null;

    public static async Task<OrderDetailsVM> CreateFromContext(OrderDto orderDto, ThirdPartyContext context)
    {
        (var customer, var supplier) = (await context.GetById(orderDto.Customer.ThirdPartyKey) 
        , await context.GetById(orderDto.Supplier.ThirdPartyKey));

        return new(orderDto) {Customer = customer, Supplier = supplier};
    }
}