using System.Diagnostics.CodeAnalysis;
using Core.Exceptions;
using Core.Messages;
using Core.Shared;

namespace Core.Entities;

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
        init
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new AddressException(ValidationErrors.ImplementRequiredField("Departamento", "Nombre"), nameof(Name));

            field = value;
        }
    }
}