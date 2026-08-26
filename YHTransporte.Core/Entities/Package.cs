using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public record Package : IEntity<int>
{
    public Package(Product product, decimal weight, decimal productAmount)
    {
        Product = product;
        Weight = weight;
        ProductAmount = productAmount;
    }

    public int Key {get; init;}

    public Product Product {get; init;}

    public decimal Weight {get; init;}

    public decimal ProductAmount {get; init;}
}