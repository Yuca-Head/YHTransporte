using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;
using YHTransporte.Application.Shipments.Validators;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.UseCases.ModifyShipment;

public sealed class ModifyShipmentHandler(IShipmentRepository repository, IOrderRepository orderRepository,
IDeliveryRepository deliveryRepository)
{
    private readonly IShipmentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly IDeliveryRepository _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));

    /// <summary>
    /// Cambia conductor, vehículo, fecha estimada de recogida y descripción de un envío que aún no ha partido.
    /// </summary>
    /// <remarks>
    /// El cambio de "In Use" (liberar el conductor/vehículo anterior y ocupar el nuevo) lo hace el procedimiento UpdateShipment.
    /// </remarks>
    public async Task<OneOf<Success, OneOf.Types.NotFound, VehicleNotFound, DriverNotFound, ValidationError>>
    Handle(ModifyShipmentCommand command, CancellationToken cancellationToken = default)
    {
        var shipment = (await _repository.GetByKeysAsync([command.Code], cancellationToken)).FirstOrDefault();

        if(shipment is null)
            return new OneOf.Types.NotFound();

        var orderStatus = await _orderRepository.GetOrderStatus(shipment.OrderKey, cancellationToken);

        if(ShipmentValidator.ValidateIsEditable(shipment.Status, orderStatus) is ValidationError notEditable)
            return notEditable;

        if(ShipmentValidator.ValidateData(command.Description, command.EstimatedPickUpAt, shipment.EstimatedPickUpAt)
        is ValidationError dataError)
            return dataError;

        // Una fecha ya registrada no se puede borrar (la entidad no lo permite).
        if(command.EstimatedPickUpAt is null && shipment.EstimatedPickUpAt is not null)
            return new ValidationError(nameof(shipment.EstimatedPickUpAt), "No se puede quitar la fecha estimada de recogida, solo cambiarla");

        var delivery = await ShipmentValidator.ResolveDelivery(_deliveryRepository, command.DriverCode, command.VehicleId,
        shipment.Driver?.Code, shipment.Vehicle?.Key, cancellationToken);

        if(delivery.IsT1)
            return delivery.AsT1;
        if(delivery.IsT2)
            return delivery.AsT2;
        if(delivery.IsT3)
            return delivery.AsT3;

        var (driver, vehicle) = delivery.AsT0;

        // Restore porque la fecha ya validada puede reemplazar a una existente (el setter no lo permite).
        var modified = Shipment.Restore(shipment.Key, shipment.OrderKey, shipment.Status, driver, vehicle,
        command.Description?.Trim(), command.EstimatedPickUpAt, shipment.PickedUpAt, shipment.DeliveredAt, shipment.Packages);

        await _repository.UpdateShipment(modified, cancellationToken);

        return new Success();
    }
}
