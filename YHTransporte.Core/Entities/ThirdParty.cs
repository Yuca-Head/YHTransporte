
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public class ThirdParty : IEntity<int>
{
    private readonly Dictionary<int, Address> addresses  = [];

    public ThirdParty(string name)
    {
        Name = name;
    }

    internal ThirdParty(ThirdParty original)
    {
        Key = original.Key; 
        Name = original.Name;
        addresses = new(original.addresses);
    }

    public int Key {get; init;}

    public string Name
    {
        get;
        init => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new PartyException(ValidationErrors.ImplementRequiredField("Tercero", "Nombre"), nameof(Name)),
            _ => value.Trim()
        };
    }

    public IEnumerable<Address> Addresses => addresses.Values;

    public SupplierRole? Supplier {get; private set;}
    public CustomerRole? Customer {get; private set;}

    public void BecomeCustomer()
    => Customer ??= new();
    public void BecomeSupplier()
    => Supplier ??= new();

    public void AddAddresses(params IEnumerable<Address> addresses)
    {
    
        foreach(var address in addresses)
        {
            if(this.addresses.ContainsKey(address.Key))
                throw new PartyException(DomainErrors.ThirdPartyAlreadyHasAddress, nameof(address));
            
            this.addresses.Add(address.Key, address);
        }
    }

}