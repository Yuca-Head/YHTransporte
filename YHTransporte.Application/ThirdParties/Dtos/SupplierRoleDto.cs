using YHTransporte.Application.Shipments.Dtos;

namespace YHTransporte.Application.ThirdParties.Dtos;

public record SupplierRoleDto(int ThirdPartyKey, IEnumerable<ProductDto> Products);