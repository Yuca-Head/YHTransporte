using System.Runtime.InteropServices;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Orders.Results;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Orders.UseCases.UpdateOrder;

public sealed class UpdateOrderHandler(IOrderRepository repository, IShipmentRepository shipmentRepository)
{
    private readonly IOrderRepository _repository = repository;

    private readonly IShipmentRepository _shipmentRepository = shipmentRepository;
    

    public async Task<OneOf<Success, OneOf.Types.NotFound, ShipmentsUncompleted, ValidationError<OrderStatuses>>> 
    UpdateOrderStatus(string orderCode, CancellationToken cancellationToken = default)
    {
        if(! await _repository.Exists(orderCode, cancellationToken))
            return new OneOf.Types.NotFound();

        var currentStatus = await _repository.GetOrderStatus(orderCode, cancellationToken);
        if(currentStatus is OrderStatuses.Completed or OrderStatuses.Canceled)
            return new ValidationError<OrderStatuses>(currentStatus);
        
        if(currentStatus is OrderStatuses.Completed-1)
        {
            var uncompletedShipments = await _shipmentRepository.GetOrderShipments(orderCode, cancellationToken)
            .Where(x => x.Status is not ShipmentStatuses.Delivered and not ShipmentStatuses.Canceled)
            .Select(x => x.Key)
            .ToArrayAsync(cancellationToken);

            if(uncompletedShipments.Length != 0)
                return new ShipmentsUncompleted(uncompletedShipments);
        }
            
        
        await _repository.ChangeOrderState(orderCode, cancellationToken);

        return new Success();
    }


    public async Task<OneOf<Success, OneOf.Types.NotFound, ShipmentsUncompleted, ValidationError>> CancelateOrder(string orderCode, CancellationToken cancellationToken = default)
    {
        if(! await _repository.Exists(orderCode, cancellationToken))
            return new OneOf.Types.NotFound();
        
        if(await _repository.GetOrderStatus(orderCode, cancellationToken) is OrderStatuses.Canceled or OrderStatuses.Completed)
            return new ValidationError("Status");

        if(!await _repository.CancelateOrderSafe(orderCode, cancellationToken))
            return new ShipmentsUncompleted();

        return new Success();
    }
}