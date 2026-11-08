using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.Shipments.Validators;

/// <summary>
/// Validaciones compartidas por los casos de uso de envíos y paquetes.
/// </summary>
internal static class ShipmentValidator
{
    // Largos de las columnas en SQL (Shipments.Description NVARCHAR(120)).
    internal const int DescriptionMaxLength = 120;

    internal static bool OrderIsDone(OrderStatuses status)
    => status is OrderStatuses.Completed or OrderStatuses.Canceled;

    /// <summary>Datos simples del envío: descripción y fecha estimada de recogida.</summary>
    internal static ValidationError? ValidateData(string? description, DateTimeOffset? estimatedPickUpAt,
    DateTimeOffset? currentEstimatedPickUpAt = null)
    {
        List<string> errors = [];

        if(description is not null && description.Trim().Length > DescriptionMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Envío", "Descripción", DescriptionMaxLength));

        // Misma regla que el setter de Shipment.EstimatedPickUpAt; solo se revisa si la fecha cambió.
        if(estimatedPickUpAt is DateTimeOffset date && date != currentEstimatedPickUpAt &&
        date < DateTimeOffset.UtcNow.AddDays(-1))
            errors.Add(ValidationErrors.LowerDateThanAllowed
            (new("Envío", "Fecha estimada de recogida", date, DateTimeOffset.UtcNow)));

        return errors.Count == 0 ? null : new ValidationError(nameof(Shipment), errors);
    }

    /// <summary>
    /// Busca el conductor y el vehículo pedidos y revisa que estén libres.
    /// Si el envío ya los tenía asignados (<paramref name="currentDriverCode"/> / <paramref name="currentVehicleId"/>) no se exige que estén libres.
    /// </summary>
    internal static async Task<OneOf<(Driver? Driver, Vehicle? Vehicle), DriverNotFound, VehicleNotFound, ValidationError>>
    ResolveDelivery(IDeliveryRepository repository, string? driverCode, int? vehicleId,
    string? currentDriverCode = null, int? currentVehicleId = null, CancellationToken cancellationToken = default)
    {
        Vehicle? vehicle = null;
        Driver? driver = null;

        if(vehicleId is int id)
        {
            vehicle = await repository.GetVehicleAsync(id, cancellationToken);

            if(vehicle is null)
                return new VehicleNotFound(id);

            if(vehicle.InUse && id != currentVehicleId)
                return new ValidationError(nameof(vehicle.InUse), DomainErrors.VehicleInUse);
        }

        if(!string.IsNullOrWhiteSpace(driverCode))
        {
            driver = await repository.GetDriverAsync(driverCode, cancellationToken);

            if(driver is null)
                return new DriverNotFound(driverCode);

            if(driver.InUse && !string.Equals(driver.Code, currentDriverCode?.Trim(), StringComparison.OrdinalIgnoreCase))
                return new ValidationError(nameof(driver.InUse), DomainErrors.DriverInUse);
        }

        return (driver, vehicle);
    }

    /// <summary>
    /// Un envío solo se modifica (y se le agregan/modifican paquetes) si aún no ha partido
    /// y su orden no está terminada ni cancelada.
    /// </summary>
    internal static ValidationError? ValidateIsEditable(ShipmentStatuses shipmentStatus, OrderStatuses orderStatus)
    {
        if(shipmentStatus is not ShipmentStatuses.Pending)
            return new ValidationError("Status", DomainErrors.ShipmentAlreadyDeparted);

        if(OrderIsDone(orderStatus))
            return new ValidationError("Status", DomainErrors.OrderIsDone);

        return null;
    }
}
