using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;
using YHTransporte.Application.Shipments.Validators;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Exceptions;

namespace YHTransporte.Application.Shipments.UseCases.CreateShipment;

public sealed class CreateShipmentHandler(IShipmentRepository repository, IOrderRepository orderRepository, IDeliveryRepository deliveryRepository)
{
    private readonly IShipmentRepository _repostiory = repository ?? throw new ArgumentNullException("Must add this");

    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException("Must add this");
    private readonly IDeliveryRepository _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException("Must add this");

    public async Task<OneOf<Success, OneOf.Types.NotFound,
    VehicleNotFound, DriverNotFound, ValidationError>> Handle(CreateShipmentCommand command, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(command.OrderCode))
            return new OneOf.Types.NotFound();

        var order = (await _orderRepository.GetByKeysAsync([command.OrderCode], cancellationToken)).FirstOrDefault();

        if(order is null)
            return new OneOf.Types.NotFound();

        if(order.Status is not Core.Enums.OrderStatuses.Pending)
            return new ValidationError(nameof(order.Status), "Solo se pueden añadir envíos a una orden cuando está pendiente. "+
            $"Estado actual: {order.Status }");

        // CAMBIO: validaciones de datos (descripción y fecha estimada) antes de tocar la base de datos.
        if(ShipmentValidator.ValidateData(command.Description, command.EstimatedPickUpAt) is ValidationError dataError)
            return dataError;

        // CAMBIO: conductor y vehículo son opcionales; si vienen, deben existir y estar libres.
        var delivery = await ShipmentValidator.ResolveDelivery
        (_deliveryRepository, command.DriverCode, command.VehicleId, cancellationToken: cancellationToken);

        if(delivery.IsT1)
            return delivery.AsT1;
        if(delivery.IsT2)
            return delivery.AsT2;
        if(delivery.IsT3)
            return delivery.AsT3;

        var (driver, vehicle) = delivery.AsT0;

        var shipment = new Shipment(driver, vehicle)
        {
            OrderKey = order.Key,
            Description = command.Description?.Trim() ?? ""
        };

        try
        {
            if(command.EstimatedPickUpAt.HasValue)
                shipment.EstimatedPickUpAt = command.EstimatedPickUpAt;
        }
        catch(ShipmentException ex)
        {
            return new ValidationError(ex.FieldName ?? nameof(shipment.EstimatedPickUpAt), ex.Message);
        }

        // El repositorio pone InUse = 1 al vehículo y al conductor dentro de la misma transacción.
        await _repostiory.AddAsync(shipment, cancellationToken);

        return new Success();
    }
}