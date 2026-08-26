using System.Dynamic;
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;


namespace YHTransporte.Core.Entities;

public class Vehicle() : IEntity<int>
{
    
    public int Key {get; init;}
    public required string Plate
    {
        get;
        init => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new DomainException(ValidationErrors.ImplementRequiredField("Vehículo", "Placa"), nameof(Plate)),
            _ => value.Trim()
        };
    }

    public string Description {get; set;} = "";
}