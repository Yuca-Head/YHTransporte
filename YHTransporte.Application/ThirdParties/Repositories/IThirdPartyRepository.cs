using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Repositories;

public interface IThirdPartyRepository : IRepository<int, ThirdParty>
{
    Task<bool> NameExists(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> FindExistingNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

    Task AddAddressToThirdParty(int addressId, int thirdPartyId, CancellationToken cancellationToken = default);


    // Products
    /// <summary>Productos de un proveedor (SupplierProducts JOIN Products). Vacío si no tiene ninguno</summary>
    IAsyncEnumerable<Product> GetSupplierProducts(int supplierId, CancellationToken cancellationToken = default);

    /// <summary>Producto con ese código (Products.Code es UNIQUE), o null si no existe </summary>
    Task<Product?> GetProductByCode(string code, CancellationToken cancellationToken = default);

    /// <summary>Inserta el producto y lo devuelve con su clave ya asignada por la base de datos</summary>
    Task<Product> AddProduct(Product product, CancellationToken cancellationToken = default);

    /// <summary>Inserta la fila (productId, supplierId) en SupplierProducts</summary>
    Task AddProductToSupplier(int productId, int supplierId, CancellationToken cancellationToken = default);
}