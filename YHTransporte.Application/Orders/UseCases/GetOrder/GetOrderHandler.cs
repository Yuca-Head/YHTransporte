using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Orders.Mappers;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Orders.UseCases.GetOrder;

public sealed class GetOrderHandler(IOrderRepository repository)
{
    private readonly IOrderRepository _repository = repository ??
    throw new ArgumentNullException();


    public async Task<OneOf<Success<OrderDto>, NotFound>> GetByCode(GetOrderQuery query, CancellationToken token = default)
    {
        var orders = (await _repository.GetByKeysAsync([query.Code], token)).ToList();

        // CAMBIO: la condición estaba invertida (devolvía NotFound justo cuando sí encontraba la orden).
        if(orders.Count == 0)
            return new NotFound();
        else
            return new Success<OrderDto>(OrderMapper.ToValue(orders.First()));
    }

    public async Task<IEnumerable<OrderDto>> GetEverythingAsync(CancellationToken cancellationToken = default)
    => (await _repository.GetEverythingAsync(cancellationToken)).Select(OrderMapper.ToValue);
}