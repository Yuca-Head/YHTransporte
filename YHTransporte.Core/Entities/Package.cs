using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public record Package : IEntity<int>
{
    // CAMBIO: el peso y la cantidad ahora se validan (en SQL son DECIMAL(10,2) e INT NOT NULL).
    public const decimal MaxWeight = 99_999_999.99m;

    public Package(Product product, decimal weight, int productAmount)
    {
        ArgumentNullException.ThrowIfNull(product);

        Product = product;
        Weight = weight;
        ProductAmount = productAmount;
    }

    public int Key {get; init;}

    public Product Product {get; init;}

    public decimal Weight
    {
        get;
        init => field = value switch
        {
            <= 0 => throw new ShipmentException(ValidationErrors.MustBePositive("Paquete", "Peso"), nameof(Weight)),
            > MaxWeight => throw new ShipmentException(ValidationErrors.GreaterThanAllowed("Paquete", "Peso", MaxWeight), nameof(Weight)),
            _ => value
        };
    }

    public int ProductAmount
    {
        get;
        init => field = value <= 0 ?
        throw new ShipmentException(ValidationErrors.MustBePositive("Paquete", "Cantidad de producto"), nameof(ProductAmount)) :
        value;
    }

    // CAMBIO: valor inicial para evitar null (antes quedaba sin asignar).
    public string ShipmentCode {get; init;} = "";
}
