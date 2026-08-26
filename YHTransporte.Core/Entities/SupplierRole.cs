
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;

namespace YHTransporte.Core.Entities;

public class SupplierRole
{
    private readonly Dictionary<int, Product> products = [];

    public IEnumerable<Product> Products => products.Values;
    public void AddProducts(params IEnumerable<Product> products)
    {
        foreach(var product in products)
        {
            if(this.products.ContainsKey(product.Key))
                throw new PartyException(DomainErrors.SupplierAlreadyHasProduct);
            
            this.products.Add(product.Key, product);
        }
    }

}