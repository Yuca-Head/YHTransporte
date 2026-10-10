using System.Diagnostics.CodeAnalysis;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;

namespace YHTransporte.Application.Addresses.Comparers;

internal sealed class CreateAddressComparer : IEqualityComparer<AddressKey>
{
    public bool Equals(AddressKey? x, AddressKey? y)
    => string.Equals(x?.Name, y?.Name, StringComparison.OrdinalIgnoreCase) &&
           x?.PlaceId == y?.PlaceId;
    public int GetHashCode([DisallowNull] AddressKey obj)
    => HashCode.Combine(
        StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name), obj.PlaceId);
}