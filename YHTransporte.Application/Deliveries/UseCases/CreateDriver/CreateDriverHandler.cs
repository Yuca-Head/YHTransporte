using OneOf;
using OneOf.Types;
using YHTransporte.Application.Deliveries.Validators;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Deliveries.UseCases.CreateDriver;

public sealed class CreateDriverHandler(IDeliveryRepository repository)
{
    private readonly IDeliveryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<Success, ValidationError>> Handle(CreateDriverCommand command, CancellationToken cancellationToken = default)
    {
        if(DeliveryValidator.ValidateDriver(command.Name, command.MiddleName, command.LastName, command.SecondLastName, isCreation: true)
        is ValidationError error)
            return error;

        await _repository.AddDriver(new Driver(command.Name, command.MiddleName, command.LastName, command.SecondLastName),
        cancellationToken);

        return new Success();
    }
}
