namespace YHTransporte.Application.Deliveries.UseCases.CreateDriver;

/// <summary>
/// Datos para registrar un conductor. El código (DRV-0000) lo genera el procedimiento InsertDriver.
/// </summary>
public sealed record CreateDriverCommand
{
    public CreateDriverCommand(string name = "", string middleName = "", string lastName = "", string secondLastName = "")
    {
        Name = name;
        MiddleName = middleName;
        LastName = lastName;
        SecondLastName = secondLastName;
    }

    public string Name {get; init => field = value?.Trim() ?? "";} = "";
    public string MiddleName {get; init => field = value?.Trim() ?? "";} = "";
    public string LastName {get; init => field = value?.Trim() ?? "";} = "";
    public string SecondLastName {get; init => field = value?.Trim() ?? "";} = "";
}
