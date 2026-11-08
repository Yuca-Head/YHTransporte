namespace YHTransporte.Application.Shipments.Dtos;

public sealed record DriverDto(string Code, string Name, string? MiddleName, string LastName, string? SecondLastName, bool InUse)
{
    // CAMBIO: nombre completo para mostrar en listas y ComboBox.
    public string FullName => string.Join(" ",
    new[] { Name, MiddleName, LastName, SecondLastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
