namespace YHTransporte.Application.ThirdParties.UseCases.ModifySupplierProduct;

/// <param name="SupplierId">Proveedor desde el que se edita (debe vender el producto).</param>
/// <param name="ProductId">Producto a modificar.</param>
public sealed record ModifySupplierProductCommand(int SupplierId, int ProductId, string Code, string Name);
