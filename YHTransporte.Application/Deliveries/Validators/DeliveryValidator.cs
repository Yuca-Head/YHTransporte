using YHTransporte.Application.Shared.Results;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.Deliveries.Validators;

/// <summary>
/// Validaciones de datos de vehículos y conductores. Los largos son los de las columnas en SQL.
/// </summary>
internal static class DeliveryValidator
{
    internal const int PlateMaxLength = 25;           // Vehicles.Plate NVARCHAR(25)
    internal const int VehicleDescriptionMaxLength = 120; // Vehicles.Description NVARCHAR(120)
    internal const int NameMaxLength = 20;            // Drivers.Name / MiddleName NVARCHAR(20)
    internal const int LastNameMaxLength = 15;        // Drivers.LastName / SecondLastName NVARCHAR(15)

    internal static ValidationError? ValidateVehicle(string? plate, string? description, bool isCreation)
    {
        List<string> errors = [];

        if(string.IsNullOrWhiteSpace(plate))
            errors.Add(isCreation ? ValidationErrors.ImplementRequiredField("Vehículo", "Placa") :
            ValidationErrors.ChangeRequiredField("Vehículo", "Placa"));
        else if(plate.Trim().Length > PlateMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Vehículo", "Placa", PlateMaxLength));

        if(description is not null && description.Trim().Length > VehicleDescriptionMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Vehículo", "Descripción", VehicleDescriptionMaxLength));

        return errors.Count == 0 ? null : new ValidationError("Vehicle", errors);
    }

    internal static ValidationError? ValidateDriver(string? name, string? middleName, string? lastName, string? secondLastName,
    bool isCreation)
    {
        List<string> errors = [];

        string Required(string field)
        => isCreation ? ValidationErrors.ImplementRequiredField("Conductor", field) :
        ValidationErrors.ChangeRequiredField("Conductor", field);

        if(string.IsNullOrWhiteSpace(name))
            errors.Add(Required("Nombre"));
        else if(name.Trim().Length > NameMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Conductor", "Nombre", NameMaxLength));

        if(middleName is not null && middleName.Trim().Length > NameMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Conductor", "Segundo nombre", NameMaxLength));

        if(string.IsNullOrWhiteSpace(lastName))
            errors.Add(Required("Apellido"));
        else if(lastName.Trim().Length > LastNameMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Conductor", "Apellido", LastNameMaxLength));

        if(secondLastName is not null && secondLastName.Trim().Length > LastNameMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Conductor", "Segundo apellido", LastNameMaxLength));

        return errors.Count == 0 ? null : new ValidationError("Driver", errors);
    }
}
