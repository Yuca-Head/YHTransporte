using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.Mappers;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Mappers;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateSupplierProduct;

public sealed class CreateSupplierProductHandler(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// Agrega un producto a un proveedor.
    /// </summary>
    /// <remarks>
    /// Si ya existe un producto con ese código y el mismo nombre (de otro proveedor), se reutiliza y solo se enlaza.
    /// Si el código existe con otro nombre se devuelve <see cref="AlreadyExists{T}"/> con el producto existente.
    /// </remarks>
    public async Task<OneOf<Success<ProductDto>, SupplierNotFound, ValidationError,
    AlreadyContainsItem<ThirdPartyDto, string>, AlreadyExists<ProductDto>>>
    Handle(CreateSupplierProductCommand command, CancellationToken cancellationToken = default)
    {
        if(ProductValidator.Validate(command.Code, command.Name, isCreation: true) is ValidationError error)
            return error;

        string code = command.Code.Trim();
        string name = command.Name.Trim();

        var supplier = (await _repository.GetByKeysAsync([command.SupplierId], cancellationToken)).FirstOrDefault();

        if(supplier?.Supplier is null)
            return new SupplierNotFound(command.SupplierId);

        bool alreadyHasIt = await _repository.GetSupplierProducts(supplier.Key, cancellationToken)
        .AnyAsync(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase), cancellationToken);

        if(alreadyHasIt)
            return new AlreadyContainsItem<ThirdPartyDto, string>(ThirdPartyDatilsMapper.ToValue(supplier), code);

        var product = await _repository.GetProductByCode(code, cancellationToken);

        if(product is not null && !string.Equals(product.Name, name, StringComparison.OrdinalIgnoreCase))
            return new AlreadyExists<ProductDto>(ProductMapper.ToValue(product));

        // Producto nuevo: se guarda primero para obtener su Id.
        product ??= await _repository.AddProduct(new Product(code, name), cancellationToken);

        await _repository.AddProductToSupplier(product.Key, supplier.Key, cancellationToken);

        return new Success<ProductDto>(ProductMapper.ToValue(product));
    }
}
