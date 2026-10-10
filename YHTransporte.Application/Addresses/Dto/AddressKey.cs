namespace YHTransporte.Application.Addresses.Dto;

/// <summary>
/// Command for creating an address of any kind.
/// </summary>
/// <param name="Name">Name or Description of the represented place</param>
/// <param name="Place">Place that contains this direction</param>
/// <remarks>
/// <paramref name="PlaceId"/> does not apply for departments.
/// </remarks>
public sealed record AddressKey
{
    public AddressKey(string name, int placeId)
    {
        Name = name;
        PlaceId = placeId;
    }
    public string Name{get; init => field = value.Trim();} = "";
    public int PlaceId {get; init;}
}