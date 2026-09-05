namespace YHTransporte.Application.Addresses.UseCases.CreateAddress;

public sealed record CreateAddressCommand(string Details, int DepartmentId);