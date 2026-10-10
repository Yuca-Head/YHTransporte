using YHTransporte.Core.Exceptions;

namespace YHTransporte.Application.Orders.Dtos;

public readonly record struct ShipmentInitialInfo
{
    public string Code{get; init
        {
            if(value.Length != 12)
                throw new OrderException("La longitud del código debe ser de 12");
            field = value;
        }
    }

    
}