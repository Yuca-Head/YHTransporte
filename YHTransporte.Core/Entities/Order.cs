using System.Collections;
using YHTransporte.Core.Enums;
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public class Order : IEntity<string>
{


    private readonly Dictionary<string, Shipment> _shipments = [];
    
    public IEnumerable<Shipment> Shipments => _shipments.Values;

    public Order(SupplierRole supplier, CustomerRole customer, Address origin, Address destination)
    {
        ArgumentNullException.ThrowIfNull(supplier);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);


        OriginDirection = origin;
        DestinationDirection = destination;
        Supplier = supplier;
        Customer = customer;
    }

    public string Key {get; init;} = "-1";

    public DateTimeOffset CreatedAt {get;} = DateTimeOffset.UtcNow;

    public SupplierRole Supplier {get; init;} 
    
    public CustomerRole Customer {get; init;}

    public Address OriginDirection {get; init;}
    
    public Address DestinationDirection {get; init;}

    public OrderStatuses Status {get; private set;} = OrderStatuses.Pending;

    public string Description {get; set;}  = "";


    public void AddShipments(params IEnumerable<Shipment> shipments)
    {

        if(Status is OrderStatuses.Completed or OrderStatuses.Canceled)
            throw new OrderException(DomainErrors.TryAddShipmentWhenOrderIsDone);

        ArgumentNullException.ThrowIfNull(shipments);

        foreach(var s in shipments)
        {
            ArgumentNullException.ThrowIfNull(s);

            if(_shipments.ContainsKey(s.Key))
                throw new OrderException(DomainErrors.OrderAlreadyHasShipment);
            
            _shipments.Add(s.Key, s);
        }
    }

    

}