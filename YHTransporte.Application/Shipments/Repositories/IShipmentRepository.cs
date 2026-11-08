using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Shipments.Repositories;

public interface IShipmentRepository : IRepository<string, Shipment>
{
    
    Task<string?> GetLastShipmentCode(CancellationToken cancellationToken = default);
    
    Task<ShipmentStatuses> GetShipmentStatus (string code, CancellationToken cancellationToken = default);

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
    Task AddPackage(string shipmentCode, Package package, CancellationToken cancellationToken = default);


    Task ChangeState(string shipmentCode, CancellationToken cancellationToken = default);

    Task CancelateShipment(string shipmentCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda conductor, vehículo, fecha estimada de recogida y descripción de un envío pendiente.
    /// </summary>
    /// <remarks>
    /// Libera el "In Use" del conductor/vehículo anterior y ocupa el nuevo (procedimiento UpdateShipment).
    /// </remarks>
    Task UpdateShipment(Shipment shipment, CancellationToken cancellationToken = default);

    /// <summary>Modifica producto, peso y cantidad de un paquete existente (procedimiento UpdateShipmentsDetail).</summary>
    Task UpdatePackage(string shipmentCode, Package package, CancellationToken cancellationToken = default);

}