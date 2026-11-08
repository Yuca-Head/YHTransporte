namespace YHTransporte.Application.Deliveries.UseCases.ModifyVehicle;

public sealed record ModifyVehicleCommand(int Id, string Plate, string Description);
