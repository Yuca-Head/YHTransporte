using OneOf;
using OneOf.Types;
using YHTransporte.Application.Deliveries.Validators;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Deliveries.UseCases.CreateVehicle;

public sealed class CreateVehicleHandler(IDeliveryRepository repository)
{
    private readonly IDeliveryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<Success, ValidationError, AlreadyExists<string>>>
    Handle(CreateVehicleCommand command, CancellationToken cancellationToken = default)
    {
        if(DeliveryValidator.ValidateVehicle(command.Plate, command.Description, isCreation: true) is ValidationError error)
            return error;

        if(await _repository.PlateExists(command.Plate, null, cancellationToken))
            return new AlreadyExists<string>(command.Plate);

        await _repository.AddVehicle(new Vehicle { Plate = command.Plate, Description = command.Description }, cancellationToken);

        return new Success();
    }
}
