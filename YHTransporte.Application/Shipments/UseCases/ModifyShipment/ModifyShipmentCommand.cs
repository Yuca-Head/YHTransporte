namespace YHTransporte.Application.Shipments.UseCases.ModifyShipment;

/// <summary>
/// Datos editables de un envío pendiente. Un conductor o vehículo en null significa "sin asignar".
/// </summary>
public readonly record struct ModifyShipmentCommand(string Code, string? DriverCode, int? VehicleId,
DateTimeOffset? EstimatedPickUpAt, string? Description);
