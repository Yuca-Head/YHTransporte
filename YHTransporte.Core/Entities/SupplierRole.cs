using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;

namespace YHTransporte.Core.Entities;

public class SupplierRole
{

    public SupplierRole(int thirdPartyKey, IEnumerable<Product>? products = null)
    {
        ThirdPartyKey = thirdPartyKey;

        var list = products is null? [] : products.ToDictionary(x => x.Key);
        _products = list;
    }

    public int ThirdPartyKey {get; init;}

    private readonly Dictionary<int, Product> _products = [];    

    public IEnumerable<Product> Products => _products.Values;
    public void AddProducts(params IEnumerable<Product> products)
    {
        foreach(var product in products)
        {
            if(this._products.ContainsKey(product.Key))
                throw new PartyException(DomainErrors.SupplierAlreadyHasProduct);
            
            this._products.Add(product.Key, product);
        }
    }

}