using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.Mappers;

internal sealed class ProductMapper : IMapper<ProductDto, Product>
{
    public static Product ToEntity(ProductDto value)
    => new(value.Code, value.Name) {Key = value.Key};

    public static ProductDto ToValue(Product entity)
    => new(entity.Key, entity.Code, entity.Name);
}