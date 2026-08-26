using YHTransporte.Core.Enums;
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public class Shipment : IEntity<string>
{

    public Shipment(Driver driver, Vehicle vehicle)
    {
        Driver = driver;
        Vehicle = vehicle;
    }

    private readonly Dictionary<int, Package> _packages = [];

    public IEnumerable<Package> Packages => _packages.Values;

    public string Key {get;}
    
    public Vehicle Vehicle{get; init;}
    public Driver Driver {get; init;}
    public DateTimeOffset? EstimatedPickUpAt 
    {
        get;

        set
        {
            if (!value.HasValue)
                throw new ArgumentNullException(nameof(value));
            

            if (field is not null) 
                throw new ShipmentException(ValidationErrors.DateIsAlreadyCreated
                (new("Envío", "Fecha estimada de recogida", default, field.Value)), nameof(EstimatedPickUpAt));
        
            if(value < DateTimeOffset.UtcNow.AddDays(-1))
                throw new ShipmentException(ValidationErrors.LowerDateThanAllowed
                    (new("Envío", "Fecha estimada de recogida", value.Value, DateTimeOffset.UtcNow)), nameof(EstimatedPickUpAt));

            field = value;
        }
    }
    public DateTimeOffset? PickedUpAt 
    {
        get;
        set
        {
            if (!value.HasValue)
                throw new ArgumentNullException(nameof(value));
            

            if(field is not null) 
                throw new ShipmentException(ValidationErrors.DateIsAlreadyCreated(new("Envío", "Fecha de recogida", default, field.Value)), nameof(PickedUpAt));
            
            if(value < DateTimeOffset.UtcNow.AddDays(-7)) //Can have a week to registrate it to the system
                throw new ShipmentException(ValidationErrors.LowerDateThanAllowed
                    (new("Envío", "Fecha de recogida", value.Value, DateTimeOffset.UtcNow)), nameof(PickedUpAt));
      
            field = value;
        }
    }
    public DateTimeOffset? DeliveredAt 
    {
        get;
        set
        {
            if (!value.HasValue)
                throw new ArgumentNullException(nameof(value));
            
            
            if(field is not null)
                throw new ShipmentException(ValidationErrors.DateIsAlreadyCreated
                (new("Envío", "Fecha de entrega", default, field.Value)));

            if(PickedUpAt is null)
                throw new ShipmentException(DomainErrors.TryToSetDeliveredDateWithoutPickedUpDateOnShipment, nameof(DeliveredAt));

            if(value < PickedUpAt)
                throw new ShipmentException(ValidationErrors.LowerDateThanAllowed
                (new("Envío", "Fecha de entrega", value ?? default, DateTimeOffset.UtcNow)), nameof(DeliveredAt));

            field = value;

        
        }
    }

    public ShipmentStatuses Status {get; private set;} = ShipmentStatuses.Pending;

    public string Description {get; set;} = "";
    

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