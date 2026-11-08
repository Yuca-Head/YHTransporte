using System.Runtime.InteropServices;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Orders.Results;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Enums;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.Orders.UseCases.UpdateOrder;

public sealed class UpdateOrderHandler(IOrderRepository repository, IShipmentRepository shipmentRepository)
{
    private readonly IOrderRepository _repository = repository;

    private readonly IShipmentRepository _shipmentRepository = shipmentRepository;


    /// <summary>
    /// Pasa la orden al siguiente estado: Pending (en espera) -> InProgress (en tránsito) -> Completed.
    /// </summary>
    /// <remarks>
    /// Reglas: para pasar a tránsito debe tener al menos un envío activo (no cancelado);
    /// para terminarla, todos sus envíos deben estar entregados o cancelados.
    /// </remarks>
    public async Task<OneOf<Success, OneOf.Types.NotFound, ShipmentsUncompleted, ValidationError<OrderStatuses>>>
    UpdateOrderStatus(string orderCode, CancellationToken cancellationToken = default)
    {
        if(! await _repository.Exists(orderCode, cancellationToken))
            return new OneOf.Types.NotFound();

        var currentStatus = await _repository.GetOrderStatus(orderCode, cancellationToken);
        if(currentStatus is OrderStatuses.Completed or OrderStatuses.Canceled)
            return new ValidationError<OrderStatuses>(currentStatus, DomainErrors.OrderIsDone);

        // CAMBIO: nueva regla, una orden sin envíos activos no puede pasar a tránsito.
        if(currentStatus is OrderStatuses.Pending)
        {
            bool hasActiveShipments = await _shipmentRepository.GetOrderShipments(orderCode, cancellationToken)
            .AnyAsync(x => x.Status is not ShipmentStatuses.Canceled, cancellationToken);

            if(!hasActiveShipments)
                return new ValidationError<OrderStatuses>(currentStatus, DomainErrors.OrderNeedsActiveShipments);
        }

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


    /// <summary>
    /// Cancela la orden y sus envíos pendientes. Si algún envío va en camino no cancela nada
    /// y devuelve los códigos de esos envíos (<see cref="ShipmentsUncompleted"/>).
    /// </summary>
    public async Task<OneOf<Success, OneOf.Types.NotFound, ShipmentsUncompleted, ValidationError>> CancelateOrder(string orderCode, CancellationToken cancellationToken = default)
    {
        if(! await _repository.Exists(orderCode, cancellationToken))
            return new OneOf.Types.NotFound();

        if(await _repository.GetOrderStatus(orderCode, cancellationToken) is OrderStatuses.Canceled or OrderStatuses.Completed)
            return new ValidationError("Status", DomainErrors.OrderIsDone);

        // CAMBIO: se revisan antes los envíos en camino para poder decirle a la interfaz cuáles son
        // (antes se devolvía ShipmentsUncompleted vacío).
        var onWayShipments = await _shipmentRepository.GetOrderShipments(orderCode, cancellationToken)
        .Where(x => x.Status is ShipmentStatuses.OnWay)
        .Select(x => x.Key)
        .ToArrayAsync(cancellationToken);

        if(onWayShipments.Length != 0)
            return new ShipmentsUncompleted(onWayShipments);

        if(!await _repository.CancelateOrderSafe(orderCode, cancellationToken))
            return new ShipmentsUncompleted();

        return new Success();
    }

    /// <summary>
    /// Cancela la orden y TODOS sus envíos que no estén entregados, también los que van en camino.
    /// </summary>
    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>> ForceCancelateOrder(string orderCode, CancellationToken cancellationToken = default)
    {
        if(! await _repository.Exists(orderCode, cancellationToken))
            return new OneOf.Types.NotFound();

        if(await _repository.GetOrderStatus(orderCode, cancellationToken) is OrderStatuses.Canceled or OrderStatuses.Completed)
            return new ValidationError("Status", DomainErrors.OrderIsDone);

        await _repository.CancelateOrderUnsafe(orderCode, cancellationToken);

        return new Success();
    }
}