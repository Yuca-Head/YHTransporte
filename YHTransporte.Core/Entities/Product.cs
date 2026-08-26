
using YHTransporte.Core.Exceptions;
using YHTransporte.Core.Messages;
using YHTransporte.Core.Shared;

namespace YHTransporte.Core.Entities;

public class Product : IEntity<int>
{
    public Product(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public int Key {get; init;}
    public string Code
    {
        get;
        init => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new ProductException(ValidationErrors.ImplementRequiredField("Producto", "Código"), nameof(Code)),
            _ => value
        };
        
    }
    public string Name 
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) switch
        {
            true => throw new ProductException
            (Name.Length > 0 ? ValidationErrors.ChangeRequiredField("Producto", "Nombre") : 
            ValidationErrors.ImplementRequiredField("Producto", "Nombre"), nameof(Name)),

            _ => value.Trim()
        };
           
    }
}