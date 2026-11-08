namespace YHTransporte.Application.Deliveries.UseCases.ModifyDriver;

public sealed record ModifyDriverCommand(string Code, string Name, string? MiddleName, string LastName, string? SecondLastName);
