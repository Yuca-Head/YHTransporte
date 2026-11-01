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
    /// Cancelates an order.
    /// </summary>
    /// <param name="orderCode"></param>
    /// <returns>Nothing</returns>
    /// <remarks>
    /// also cancelates order's shipments if they are not completed.
    /// </remarks>
    Task CancelateOrder(string orderCode, CancellationToken cancellationToken = default);


    Task<OrderStatuses> GetOrderStatus(string orderCode, CancellationToken cancellationToken = default);
    
    Task<string?> GetLastCode(CancellationToken cancellationToken = default);

    

}