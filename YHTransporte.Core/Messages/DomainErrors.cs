namespace YHTransporte.Core.Messages;

public partial class DomainErrors
{

    #region ThirdPartyErrors
    public const string ThirdPartyAlreadyHasAddress = "Este tercero ya posee esta dirección";

    public const string SupplierAlreadyHasProduct = "Este proveedor ya tiene este product";


    #endregion





    #region ShipmentErrors

    public const string TryToSetDeliveredDateWithoutPickedUpDateOnShipment =
    "No se puede realizar entrega sin que haber recogido la carga";

    public const string ShipmentAlreadyHasPackage =
    "Este envío ya contiene un paquete con el mismo número de Id";

    public const string ShipmentAlreadyDeparted =
    "El envío ya partió (o terminó), solo se puede modificar mientras está pendiente";

    public const string ShipmentNeedsDriverAndVehicle =
    "El envío necesita un conductor y un vehículo antes de ponerse en camino";

    public const string ShipmentNeedsPackages =
    "El envío debe tener al menos un paquete antes de ponerse en camino";

    public const string ShipmentNeedsOrderInProgress =
    "La orden debe estar en tránsito para que un envío pueda ponerse en camino";

    public const string ProductDoesNotBelongToSupplier =
    "El producto no pertenece al proveedor de la orden";

    #endregion





    #region DeliveryErrors

    public const string VehicleInUse = "El vehículo ya se encuentra en uso";

    public const string DriverInUse = "El conductor ya se encuentra en un envío";

    public const string CannotChangePlateWhileInUse =
    "No se puede cambiar la placa de un vehículo mientras está en uso";

    #endregion





    #region OrderErrors

    public const string OrderAlreadyHasShipment = 
    "Esta orden ya tiene esta orden";

    public const string TryAddShipmentWhenOrderIsDone =
    "No se puede encargar un envío cuando una orden está terminada/cancelada";

    public const string OrderIsDone =
    "La orden ya está terminada o cancelada, no se puede modificar";

    public const string OrderNeedsActiveShipments =
    "La orden debe tener al menos un envío activo (no cancelado) para pasar a tránsito";

    #endregion





}