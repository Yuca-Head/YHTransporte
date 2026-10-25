namespace YHTransporte.Application.Orders.UseCases.CreateOrder;

public readonly record struct CreateOrderCommand(int SupplierId, int CustomerId, int OriginId, int DestinationId,
string? Description = "")
{
    public readonly DateTimeOffset Creation {get; init;} = DateTimeOffset.UtcNow;
}