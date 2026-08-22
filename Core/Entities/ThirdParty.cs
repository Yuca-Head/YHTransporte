using Core.Exceptions;
using Core.Messages;
using Core.Shared;

namespace Core.Entities;

public class ThirdParty : IEntity<int>
{
    private readonly Dictionary<int, Address> addresses  = [];

    public ThirdParty(string name)
    {
        Name = name;
    }

    public int Key {get; init;}

    public string Name
    {
        get;
        init
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new PartyException(ValidationErrors.ImplementRequiredField("Tercero", "Nombre"), nameof(Name));
            field = value;
        }
    }

    public bool IsSupplier{get; protected set;}

    public bool IsCustomer{get; protected set;}


    public void AddAddress(Address address)
    {
        if(addresses.ContainsKey(address.Key))
            throw new PartyException(DomainErrors.ThirdPartyAlreadyHasAddress, nameof(address));
        
        addresses.Add(address.Key, address);
    }

    public IEnumerable<Address> GetAddresses()
    => addresses.Values;

}