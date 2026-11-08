namespace YHTransporte.Application.Shipments.UseCases.CreateShipment;

/// <summary>
/// Comando para crear un envío dentro de una orden.
/// </summary>
/// <remarks>
/// CAMBIO: el conductor y el vehículo ahora son opcionales (en SQL aceptan NULL); se exigen
/// hasta que el envío se pone en camino. También se agregan la fecha estimada de recogida y la descripción.
/// </remarks>
public readonly record struct CreateShipmentCommand(string OrderCode, string? DriverCode = null, int? VehicleId = null,
DateTimeOffset? EstimatedPickUpAt = null, string? Description = null); 
