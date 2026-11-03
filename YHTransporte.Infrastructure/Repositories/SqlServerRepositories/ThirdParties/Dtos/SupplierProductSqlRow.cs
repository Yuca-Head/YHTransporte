namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

/// <summary>Fila de dbo.SupplierProducts (qué productos vende cada proveedor).</summary>
public sealed record SupplierProductSqlRow(int IdProduct, int IdSupplier);