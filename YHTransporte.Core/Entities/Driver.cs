using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public class Driver : IEntity<int>
{

    public Driver(string name, string? middleName, string lastName, string? secondLastName)
    {
        Name = name;
        LastName = lastName;
        MiddleName = (middleName ?? string.Empty).Trim();
        SecondLastName = (secondLastName ?? string.Empty).Trim();
    }
    public int Key {get; init;} = -1;

    public string Code 
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value) ? value.Trim() :
        throw new DomainException(ValidationErrors.ImplementRequiredField("Conductor", "Código"));
    }
    public string Name 
    {
        get; 
        init => field = !string.IsNullOrWhiteSpace(value) ? value.Trim() :
        throw new DomainException(ValidationErrors.ImplementRequiredField("Conductor", "Nombre"));
    }
    public string MiddleName {get; init;}
    public string LastName 
    {
        get; 
        init => field = !string.IsNullOrWhiteSpace(value) ? value.Trim() :
        throw new DomainException(ValidationErrors.ImplementRequiredField("Conductor", "Apellido"));
    }
    public string SecondLastName {get; init;}

    public bool InUse {get; set;}
}