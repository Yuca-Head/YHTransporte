namespace YHTransporte.Application.Shipments.UseCases.CreateShipment;

public readonly record struct CreateShipmentCommand(string OrderCode, string DriverCode, int VehicleId); 

