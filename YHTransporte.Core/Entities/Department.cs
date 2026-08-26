
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public record Department : IEntity<int>
{

    public Department(string name)
    {
        Name = name;
    }

    public int Key {get; init;}

    public string Name
    {
        get;
        init => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new AddressException(ValidationErrors.ImplementRequiredField("Departamento", "Nombre"), nameof(Name)),
            _ => value.Trim()
        };
    }
}