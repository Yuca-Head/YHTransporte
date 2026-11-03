using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Orders.Repositories;

public interface IOrderRepository : IRepository<string, Order>
{


    /// <summary>
    /// Changes order to next State.
    /// </summary>
    /// <param name="orderCode"></param>
    /// <returns>Nothing</returns>
    Task ChangeOrderState(string orderCode, CancellationToken cancellationToken = default);


    /// <summary>
    /// Cancelates an order, but only if none of its shipments is on its way.
    /// </summary>
    /// <param name="orderCode"></param>
    /// <returns>True if it could be cancelated correctly</returns>
    /// <remarks>
    /// Also cancelates the order's pending shipments. If a shipment is on its way, nothing is changed and an error is thrown.
    /// </remarks>
    Task<bool> CancelateOrderSafe(string orderCode, CancellationToken cancellationToken = default);


    /// <summary>
    /// Cancelates an order and every shipment that is not delivered yet, including the ones on their way.
    /// </summary>
    /// <param name="orderCode"></param>
    /// <returns>Nothing</returns>
    Task CancelateOrderUnsafe(string orderCode, CancellationToken cancellationToken = default);


    Task<OrderStatuses> GetOrderStatus(string orderCode, CancellationToken cancellationToken = default);
    
    Task<string?> GetLastCode(CancellationToken cancellationToken = default);

    

}