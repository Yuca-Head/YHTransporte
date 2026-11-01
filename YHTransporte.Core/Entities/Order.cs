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

    // CAMBIO: ahora la orden recibe también el Id del proveedor y del cliente (las columnas
    // SupplierId y CustomerId de la tabla Orders). Antes solo recibía los roles, que no tienen Id,
    // y por eso no se podía guardar ni reconstruir una orden desde la base de datos.
    public Order(SupplierRole supplier, CustomerRole customer,
    Address origin, Address destination)
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

    // CAMBIO: de {get;} a {get; init;} para poder restaurar la fecha que viene de la base de datos.
    public DateTimeOffset CreatedAt {get; init;} = DateTimeOffset.UtcNow;

    /// <summary>Id del tercero que es proveedor (columna Orders.SupplierId).</summary>
    public int SupplierId => Supplier.ThirdPartyKey;

    /// <summary>Id del tercero que es cliente (columna Orders.CustomerId).</summary>
    public int CustomerId => Customer.ThirdPartyKey;

    public SupplierRole Supplier {get; init;}

    public CustomerRole Customer {get; init;}

    public Address OriginDirection {get; init;}

    public Address DestinationDirection {get; init;}

    // CAMBIO: de "private set" a "init" para poder restaurar el estado que viene de la base de datos
    // (el cambio de estado real lo hace el procedimiento almacenado ChangeOrderStatus).
    public OrderStatuses Status {get; init;} = OrderStatuses.Pending;

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