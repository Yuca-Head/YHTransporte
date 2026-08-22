using Core.Exceptions;
using Core.Messages;
using Core.Shared;

namespace Core.Entities;


public class Address : IEntity<int>
{
    public Address(string details, Municipality municipality)
    {
        Details = details;
        Municipality = municipality;
    }

    public string Details
    {
        get;
        set
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new AddressException(ValidationErrors.ImplementRequiredField("Dirección", "Detalle"), nameof(Details));
            field = value;
        }
    }
    public Municipality Municipality {get; init;}

    public int Key {get;}
}