namespace YHTransporte.Application.ThirdParties.UseCases.CreateSupplierProduct;

/// <param name="SupplierId">Tercero (con rol de proveedor) que vende el producto.</param>
/// <param name="Code">Código único del producto (máx. 8 caracteres).</param>
/// <param name="Name">Nombre del producto (máx. 50 caracteres).</param>
public sealed record CreateSupplierProductCommand(int SupplierId, string Code, string Name);
