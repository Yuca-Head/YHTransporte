namespace YHTransporte.Application.Shipments.UseCases.AddPackage;

/// <param name="ShipmentCode">Envío al que se agrega el paquete.</param>
/// <param name="ProductId">Producto del proveedor de la orden.</param>
/// <param name="Weight">Peso en kg (máximo 2 decimales).</param>
/// <param name="ProductAmount">Cantidad de unidades del producto.</param>
public readonly record struct AddPackageCommand(string ShipmentCode, int ProductId, decimal Weight, int ProductAmount);
