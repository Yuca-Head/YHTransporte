using OneOf;
using OneOf.Types;
using YHTransporte.Application.Deliveries.Validators;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Deliveries.UseCases.ModifyDriver;

public sealed class ModifyDriverHandler(IDeliveryRepository repository)
{
    private readonly IDeliveryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>Cambia los nombres del conductor. El código y el estado "En uso" no se tocan.</summary>
    public async Task<OneOf<Success, DriverNotFound, ValidationError>> Handle(ModifyDriverCommand command,
    CancellationToken cancellationToken = default)
    {
        if(DeliveryValidator.ValidateDriver(command.Name, command.MiddleName, command.LastName, command.SecondLastName, isCreation: false)
        is ValidationError error)
            return error;

        if(string.IsNullOrWhiteSpace(command.Code))
            return new DriverNotFound(command.Code ?? "");

        var driver = await _repository.GetDriverAsync(command.Code, cancellationToken);

        if(driver is null)
            return new DriverNotFound(command.Code);

        // Las propiedades son "init" en la entidad: se arma un conductor nuevo con la misma clave y código.
        await _repository.UpdateDriver(new Driver(command.Name, command.MiddleName, command.LastName, command.SecondLastName)
        {
            Key = driver.Key,
            Code = driver.Code,
            InUse = driver.InUse
        }, cancellationToken);

        return new Success();
    }
}
