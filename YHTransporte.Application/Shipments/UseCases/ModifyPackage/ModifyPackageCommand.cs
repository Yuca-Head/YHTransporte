namespace YHTransporte.Application.Shipments.UseCases.ModifyPackage;

/// <param name="ShipmentCode">Envío dueño del paquete.</param>
/// <param name="PackageId">Id del paquete dentro del envío (ShipmentsDetails.Id).</param>
public readonly record struct ModifyPackageCommand(string ShipmentCode, int PackageId, int ProductId, decimal Weight, int ProductAmount);
