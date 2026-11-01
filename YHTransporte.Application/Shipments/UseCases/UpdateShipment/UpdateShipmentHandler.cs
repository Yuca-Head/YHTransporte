using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Shipments.UseCases.UpdateShipment;

public sealed class UpdateShipmentHandler
(IShipmentRepository repository, IDeliveryRepository deliveryRepository, IOrderRepository orderRepository)
{
    private readonly IShipmentRepository _repository = repository;

    private readonly IDeliveryRepository _deliveryRepository = deliveryRepository;
    
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>> ChangeShipmentState(string code, CancellationToken cancellationToken = default)
    {
        var shipment = (await _repository.GetByKeysAsync([code], cancellationToken)).FirstOrDefault();

        if(shipment is null)
            return new OneOf.Types.NotFound();
        
        if(shipment.Status is Core.Enums.ShipmentStatuses.Canceled or Core.Enums.ShipmentStatuses.Delivered)
            return new ValidationError(nameof(shipment.Status), "No se puede cambiar el estado de una orden entregada o cancelada");
        
        if(await _orderRepository.GetOrderStatus(shipment.OrderKey, cancellationToken)
        is Core.Enums.OrderStatuses.Canceled or Core.Enums.OrderStatuses.Completed)
            return new ValidationError("Status", "No se puede cambiar el estado si una orden ya está terminada o cancelada");
        
        await _repository.ChangeState(code, cancellationToken);


        return new Success();
    }

    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>> CancelateShipment(string shipmentCode, CancellationToken cancellationToken)
    {
    
        if(! await _repository.Exists(shipmentCode, cancellationToken))
            return new OneOf.Types.NotFound();
        
        if(await _repository.GetShipmentStatus(shipmentCode, cancellationToken) is ShipmentStatuses.Canceled or ShipmentStatuses.Delivered)
            return new ValidationError("Status");

        await _repository.CancelateShipment(shipmentCode, cancellationToken);
    
        return new Success();
    }

}