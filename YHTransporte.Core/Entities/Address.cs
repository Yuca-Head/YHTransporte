

using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;


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
        set => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new AddressException(ValidationErrors.ImplementRequiredField("Dirección", "Detalle"), nameof(Details)),
            _ => value.Trim()
        };
        
    }
    public Municipality Municipality {get; init;}

    public int Key {get;}
}