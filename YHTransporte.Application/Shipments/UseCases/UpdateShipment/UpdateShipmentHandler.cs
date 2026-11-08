using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Enums;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.Shipments.UseCases.UpdateShipment;

public sealed class UpdateShipmentHandler
(IShipmentRepository repository,  IOrderRepository orderRepository)
{
    private readonly IShipmentRepository _repository = repository;

    private readonly IOrderRepository _orderRepository = orderRepository;

    /// <summary>
    /// Pasa el envío al siguiente estado: Pending -> OnWay -> Delivered.
    /// </summary>
    /// <remarks>
    /// CAMBIO: para ponerse en camino ahora se exige (antes de llegar a SQL) que la orden esté en tránsito,
    /// que tenga conductor y vehículo, y que tenga al menos un paquete.
    /// </remarks>
    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>> ChangeShipmentState(string code, CancellationToken cancellationToken = default)
    {
        var shipment = (await _repository.GetByKeysAsync([code], cancellationToken)).FirstOrDefault();

        if(shipment is null)
            return new OneOf.Types.NotFound();

        if(shipment.Status is Core.Enums.ShipmentStatuses.Canceled or Core.Enums.ShipmentStatuses.Delivered)
            return new ValidationError(nameof(shipment.Status), "No se puede cambiar el estado de un envío entregado o cancelado");

        var orderStatus = await _orderRepository.GetOrderStatus(shipment.OrderKey, cancellationToken);

        if(orderStatus is Core.Enums.OrderStatuses.Canceled or Core.Enums.OrderStatuses.Completed)
            return new ValidationError("Status", "No se puede cambiar el estado si una orden ya está terminada o cancelada");

        if(shipment.Status is ShipmentStatuses.Pending)
        {
            List<string> errors = [];

            if(orderStatus is not OrderStatuses.InProgress)
                errors.Add(DomainErrors.ShipmentNeedsOrderInProgress);

            if(shipment.Driver is null || shipment.Vehicle is null)
                errors.Add(DomainErrors.ShipmentNeedsDriverAndVehicle);

            if(!shipment.Packages.Any())
                errors.Add(DomainErrors.ShipmentNeedsPackages);

            if(errors.Count != 0)
                return new ValidationError(nameof(shipment.Status), errors);
        }

        await _repository.ChangeState(code, cancellationToken);


        return new Success();
    }

    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>> CancelateShipment(string shipmentCode, CancellationToken cancellationToken = default)
    {

        if(! await _repository.Exists(shipmentCode, cancellationToken))
            return new OneOf.Types.NotFound();

        if(await _repository.GetShipmentStatus(shipmentCode, cancellationToken) is ShipmentStatuses.Canceled or ShipmentStatuses.Delivered)
            return new ValidationError("Status", "No se puede cancelar un envío entregado o ya cancelado");

        // CAMBIO: tampoco se cancela un envío de una orden terminada/cancelada.
        var shipment = (await _repository.GetByKeysAsync([shipmentCode], cancellationToken)).First();

        if(await _orderRepository.GetOrderStatus(shipment.OrderKey, cancellationToken) is OrderStatuses.Completed or OrderStatuses.Canceled)
            return new ValidationError("Status", DomainErrors.OrderIsDone);

        await _repository.CancelateShipment(shipmentCode, cancellationToken);

        return new Success();
    }

}