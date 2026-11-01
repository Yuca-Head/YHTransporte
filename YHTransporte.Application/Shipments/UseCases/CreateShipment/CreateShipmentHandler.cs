using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.UseCases.CreateShipment;

public sealed class CreateShipmentHandler(IShipmentRepository repository, IOrderRepository orderRepository, IDeliveryRepository deliveryRepository)
{
    private readonly IShipmentRepository _repostiory = repository ?? throw new ArgumentNullException("Must add this");

    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException("Must add this");
    private readonly IDeliveryRepository _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException("Must add this");

    public async Task<OneOf<Success, OneOf.Types.NotFound, 
    VehicleNotFound, DriverNotFound, ValidationError>> Handle(CreateShipmentCommand command, CancellationToken cancellationToken = default)
    {
        var order = (await _orderRepository.GetByKeysAsync([command.OrderCode], cancellationToken)).FirstOrDefault();

        if(order is null)
            return new OneOf.Types.NotFound();
        
        if(order.Status is not Core.Enums.OrderStatuses.Pending)
            return new ValidationError(nameof(order.Status), "Solo se pueden añadir envíos a una orden cuando está pendiente. "+
            $"Estado actual: {order.Status }");
        
        var vehicle = await _deliveryRepository.GetVehicleAsync(command.VehicleId, cancellationToken);
        
        if(vehicle is null)
            return new VehicleNotFound(command.VehicleId);

        if(vehicle.InUse)
            return new ValidationError(nameof(vehicle.InUse), "El vehículo ya se encuentra en uso");
        
        var driver = await _deliveryRepository.GetDriverAsync(command.DriverCode, cancellationToken);

        if(driver is null)
            return new DriverNotFound(command.DriverCode);

        if(driver.InUse)
            return new ValidationError(nameof(driver.InUse), "El conductor ya se encuentra en un envío");

        //This should change driver's and vehicle's "In Use" property... ain't? 
        await _repostiory.AddAsync(new Shipment(driver, vehicle){OrderKey = order.Key}, cancellationToken);
    
        return new Success();
    }
}