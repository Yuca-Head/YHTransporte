namespace YHTransporte.Application.Deliveries.UseCases.CreateVehicle;

public sealed record CreateVehicleCommand
{
    public CreateVehicleCommand(string plate = "", string description = "")
    {
        Plate = plate;
        Description = description;
    }

    public string Plate {get; init => field = value?.Trim() ?? "";} = "";

    public string Description {get; init => field = value?.Trim() ?? "";} = "";
}
