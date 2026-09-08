namespace YHTransporte.Application.Addresses.UseCases.GetAddress;

/// <summary>
/// General Command To Any kind of address.
/// Can be use either for addresses, departments and municipalities.
/// </summary>
public sealed record GetAddressCommand(int Id);