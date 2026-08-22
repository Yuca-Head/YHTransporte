using Core.Exceptions;
using Core.Messages;
using Core.Shared;

namespace Core.Entities;

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
        init
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new AddressException(ValidationErrors.ImplementRequiredField("Municipio", "Nombre"), nameof(Name));

            field = value;
        }
    }

    public Department Department{get; init;} 

}