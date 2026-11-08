using OneOf;
using OneOf.Types;
using YHTransporte.Application.Deliveries.Validators;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.Deliveries.UseCases.ModifyVehicle;

public sealed class ModifyVehicleHandler(IDeliveryRepository repository)
{
    private readonly IDeliveryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// Cambia placa y descripción. La placa no se puede cambiar mientras el vehículo está en un envío.
    /// </summary>
    public async Task<OneOf<Success, VehicleNotFound, ValidationError, AlreadyExists<string>>>
    Handle(ModifyVehicleCommand command, CancellationToken cancellationToken = default)
    {
        string plate = command.Plate?.Trim() ?? "";
        string description = command.Description?.Trim() ?? "";

        if(DeliveryValidator.ValidateVehicle(plate, description, isCreation: false) is ValidationError error)
            return error;

        var vehicle = await _repository.GetVehicleAsync(command.Id, cancellationToken);

        if(vehicle is null)
            return new VehicleNotFound(command.Id);

        bool plateChanged = !string.Equals(vehicle.Plate, plate, StringComparison.OrdinalIgnoreCase);

        if(plateChanged && vehicle.InUse)
            return new ValidationError(nameof(vehicle.Plate), DomainErrors.CannotChangePlateWhileInUse);

        if(plateChanged && await _repository.PlateExists(plate, vehicle.Key, cancellationToken))
            return new AlreadyExists<string>(plate);

        // La placa es "init" en la entidad: se arma una nueva con la misma clave.
        await _repository.UpdateVehicle(new Vehicle
        {
            Key = vehicle.Key,
            Plate = plate,
            Description = description,
            InUse = vehicle.InUse
        }, cancellationToken);

        return new Success();
    }
}
