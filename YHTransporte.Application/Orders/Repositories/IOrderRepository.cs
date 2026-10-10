using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Orders.Repositories;

public interface IOrderRepository : IRepository<string, Order>
{
    Task AddShipment(string orderCode, Shipment shipment, CancellationToken cancellationToken = default);

    Task AddPackage(string shipmentCode, Package package, CancellationToken cancellationToken = default);
    
    Task<string> GetLastCode(CancellationToken cancellationToken = default);

    Task<string> GetLastShipmentCode(CancellationToken cancellationToken = default);
    
    

    /// <summary>
    /// 
    /// </summary> 
    /// <param name="shipmentId"></param>
    /// <remarks>
    /// PLS USE "YIELD RETURN" WHEN IMPLEMENTATE.
    /// </remarks>
    /// <returns></returns>
    IAsyncEnumerable<Package> GetShipmentPackages(string shipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="orderId"></param>
    /// <remarks>
    /// PLS USE "YIELD RETURN" WHEN IMPLEMENTATE.
    /// </remarks>
    /// <returns></returns>
    IAsyncEnumerable<Shipment> GetOrderShipments(string orderId, CancellationToken cancellationToken = default); 


    

}