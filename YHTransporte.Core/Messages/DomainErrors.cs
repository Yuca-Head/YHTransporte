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

    #endregion





    #region OrderErrors

    public const string OrderAlreadyHasShipment = 
    "Esta orden ya tiene esta orden";

    public const string TryAddShipmentWhenOrderIsDone = 
    "No se puede encargar un envío cuando una orden está terminada/cancelada";

    #endregion





}