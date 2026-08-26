using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public record Municipality : IEntity<int>
{

    public Municipality(string name, Department department)
    {
        Name = name;
        Department = department;
    }

    public int Key {get; init;}

    public string Name
    {
        get;
        init => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new AddressException(ValidationErrors.ImplementRequiredField("Municipio", "Nombre"), nameof(Name)),
            _ => value.Trim()
        };
        
    }

    public Department Department{get; init;} 

}