namespace YHTransporte.Application.Shipments.Dtos;

public sealed record DriverDto(string Code, string Name, string? MiddleName, string LastName, string? SecondLastName, bool InUse);