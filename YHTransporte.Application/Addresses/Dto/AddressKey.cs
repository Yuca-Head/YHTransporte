namespace YHTransporte.Application.Addresses.Dto;

/// <summary>
/// Command for creating an address of any kind.
/// </summary>
/// <param name="Name">Name or Description of the represented place</param>
/// <param name="Place">Place that contains this direction</param>
/// <remarks>
/// <paramref name="PlaceId"/> does not apply for departments.
/// </remarks>
public sealed record AddressKey(string Name, int PlaceId);