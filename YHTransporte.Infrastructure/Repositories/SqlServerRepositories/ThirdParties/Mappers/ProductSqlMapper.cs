using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Mappers;

public class ProductSqlMapper : IMapper<ProductSqlRow, Product>
{
    public static Product ToEntity(ProductSqlRow value)
    => new(value.Code, value.Name) { Key = value.Id };

    public static ProductSqlRow ToValue(Product entity)
    => new(entity.Key, entity.Code, entity.Name);
}