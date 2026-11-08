using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.Mappers;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Application.ThirdParties.UseCases.CreateSupplierProduct;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.UseCases.ModifySupplierProduct;

public sealed class ModifySupplierProductHandler(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// Cambia código y nombre de un producto del proveedor.
    /// </summary>
    /// <remarks>
    /// El producto es compartido: si otro proveedor también lo vende, el cambio se verá allí también.
    /// </remarks>
    public async Task<OneOf<Success<ProductDto>, SupplierNotFound, NotFound<int>, ValidationError, AlreadyExists<ProductDto>>>
    Handle(ModifySupplierProductCommand command, CancellationToken cancellationToken = default)
    {
        if(ProductValidator.Validate(command.Code, command.Name, isCreation: false) is ValidationError error)
            return error;

        string code = command.Code.Trim();
        string name = command.Name.Trim();

        var supplier = (await _repository.GetByKeysAsync([command.SupplierId], cancellationToken)).FirstOrDefault();

        if(supplier?.Supplier is null)
            return new SupplierNotFound(command.SupplierId);

        var product = await _repository.GetSupplierProducts(supplier.Key, cancellationToken)
        .FirstOrDefaultAsync(x => x.Key == command.ProductId, cancellationToken);

        if(product is null)
            return new NotFound<int>(command.ProductId);

        // Products.Code es UNIQUE: el nuevo código no puede pertenecer a otro producto.
        if(!string.Equals(product.Code, code, StringComparison.OrdinalIgnoreCase))
        {
            var sameCode = await _repository.GetProductByCode(code, cancellationToken);

            if(sameCode is not null && sameCode.Key != product.Key)
                return new AlreadyExists<ProductDto>(ProductMapper.ToValue(sameCode));
        }

        // El código es "init" en la entidad: se arma un producto nuevo con la misma clave.
        var modified = new Product(code, name) { Key = product.Key };

        await _repository.UpdateProduct(modified, cancellationToken);

        return new Success<ProductDto>(ProductMapper.ToValue(modified));
    }
}
