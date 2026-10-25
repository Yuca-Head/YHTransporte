using YHTransporte.Core.Enums;
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;


public class Shipment : IEntity<string>
{

    // CAMBIO: Driver y Vehicle ahora pueden ser null. En la base de datos las columnas
    // Shipments.IdDriver e IdVehicle aceptan NULL (un envío "Pending" aún puede no tener
    // conductor ni vehículo; el procedimiento ChangeShipmentStatus los exige hasta pasar a "On way").
    public Shipment(Driver? driver = null, Vehicle? vehicle = null  )
    {
        Driver = driver;
        Vehicle = vehicle;
    }

    private readonly Dictionary<int, Package> _packages = [];

    public IEnumerable<Package> Packages => _packages.Values;

    // CAMBIO: antes era {get;} sin valor (siempre null). Ahora se puede asignar el código del envío
    // (columna Shipments.Code) y, igual que Order, empieza en "-1" mientras no se ha guardado.
    public string Key {get; init;} = "-1";

    /// <summary>Código de la orden a la que pertenece (columna Shipments.CodeOrder). Se asigna al restaurar desde la BD.</summary>
    public string? OrderKey {get; init;}

    public Vehicle? Vehicle{get; init;}
    public Driver? Driver {get; init;}

    // CAMBIO: nuevo estado de la tabla ShipmentStatuses; solo se cambia desde dentro de la clase.
    public ShipmentStatuses Status {get; private set;} = ShipmentStatuses.Pending;

    public string Description {get; set;} = "";

    // CAMBIO: las 3 fechas ahora usan campos privados explícitos (_estimatedPickUpAt, etc.) en lugar
    // de la palabra "field". Es lo único que permite que el método Restore (abajo) pueda llenarlas
    // sin pasar por las validaciones. Las validaciones de los setters quedan EXACTAMENTE igual.
    private DateTimeOffset? _estimatedPickUpAt;
    private DateTimeOffset? _pickedUpAt;
    private DateTimeOffset? _deliveredAt;

    public DateTimeOffset? EstimatedPickUpAt
    {
        get => _estimatedPickUpAt;

        set
        {
            if (!value.HasValue)
                throw new ArgumentNullException(nameof(value));


            if (_estimatedPickUpAt is not null)
                throw new ShipmentException(ValidationErrors.DateIsAlreadyCreated
                (new("Envío", "Fecha estimada de recogida", default, _estimatedPickUpAt.Value)), nameof(EstimatedPickUpAt));

            if(value < DateTimeOffset.UtcNow.AddDays(-1))
                throw new ShipmentException(ValidationErrors.LowerDateThanAllowed
                    (new("Envío", "Fecha estimada de recogida", value.Value, DateTimeOffset.UtcNow)), nameof(EstimatedPickUpAt));

            _estimatedPickUpAt = value;
        }
    }
    public DateTimeOffset? PickedUpAt
    {
        get => _pickedUpAt;
        set
        {
            if (!value.HasValue)
                throw new ArgumentNullException(nameof(value));


            if(_pickedUpAt is not null)
                throw new ShipmentException(ValidationErrors.DateIsAlreadyCreated(new("Envío", "Fecha de recogida", default, _pickedUpAt.Value)), nameof(PickedUpAt));

            if(value < DateTimeOffset.UtcNow.AddDays(-7)) //Can have a week to registrate it to the system
                throw new ShipmentException(ValidationErrors.LowerDateThanAllowed
                    (new("Envío", "Fecha de recogida", value.Value, DateTimeOffset.UtcNow)), nameof(PickedUpAt));

            _pickedUpAt = value;
        }
    }
    public DateTimeOffset? DeliveredAt
    {
        get => _deliveredAt;
        set
        {
            if (!value.HasValue)
                throw new ArgumentNullException(nameof(value));


            if(_deliveredAt is not null)
                throw new ShipmentException(ValidationErrors.DateIsAlreadyCreated
                (new("Envío", "Fecha de entrega", default, _deliveredAt.Value)));

            if(PickedUpAt is null)
                throw new ShipmentException(DomainErrors.TryToSetDeliveredDateWithoutPickedUpDateOnShipment, nameof(DeliveredAt));

            if(value < PickedUpAt)
                throw new ShipmentException(ValidationErrors.LowerDateThanAllowed
                (new("Envío", "Fecha de entrega", value ?? default, DateTimeOffset.UtcNow)), nameof(DeliveredAt));

            _deliveredAt = value;


        }
    }

   
    internal static Shipment Restore(
        string key,
        string orderKey,
        ShipmentStatuses status,
        Driver? driver,
        Vehicle? vehicle,
        string? description,
        DateTimeOffset? estimatedPickUpAt,
        DateTimeOffset? pickedUpAt,
        DateTimeOffset? deliveredAt)
    {
        return new Shipment(driver, vehicle)
        {
            Key = key,
            OrderKey = orderKey,
            Description = description ?? "",
            Status = status,
            _estimatedPickUpAt = estimatedPickUpAt,
            _pickedUpAt = pickedUpAt,
            _deliveredAt = deliveredAt
        };
    }



    public void AddPackages(params IEnumerable<Package> packages)
    {
        ArgumentNullException.ThrowIfNull(packages);
        foreach(var p in packages)
        {
            ArgumentNullException.ThrowIfNull(p);

            if (_packages.ContainsKey(p.Key))
                throw new ShipmentException(
                    DomainErrors.ShipmentAlreadyHasPackage);

            _packages.Add(p.Key, p);
        }
    }

}