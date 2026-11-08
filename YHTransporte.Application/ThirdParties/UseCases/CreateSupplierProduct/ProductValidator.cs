using YHTransporte.Application.Shared.Results;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateSupplierProduct;

/// <summary>
/// Validaciones de datos de un producto. Los largos son los de dbo.Products (Code NVARCHAR(8), Name NVARCHAR(50)).
/// </summary>
internal static class ProductValidator
{
    internal const int CodeMaxLength = 8;
    internal const int NameMaxLength = 50;

    internal static ValidationError? Validate(string? code, string? name, bool isCreation)
    {
        List<string> errors = [];

        string Required(string field)
        => isCreation ? ValidationErrors.ImplementRequiredField("Producto", field) :
        ValidationErrors.ChangeRequiredField("Producto", field);

        if(string.IsNullOrWhiteSpace(code))
            errors.Add(Required("Código"));
        else if(code.Trim().Length > CodeMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Producto", "Código", CodeMaxLength));

        if(string.IsNullOrWhiteSpace(name))
            errors.Add(Required("Nombre"));
        else if(name.Trim().Length > NameMaxLength)
            errors.Add(ValidationErrors.ExceedsMaxLength("Producto", "Nombre", NameMaxLength));

        return errors.Count == 0 ? null : new ValidationError("Product", errors);
    }
}
