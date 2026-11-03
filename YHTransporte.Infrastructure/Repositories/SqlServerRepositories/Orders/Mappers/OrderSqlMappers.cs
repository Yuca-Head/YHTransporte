using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Mappers;

/// <summary>
/// Convierte los Id de estado de SQL (OrderStatuses / ShipmentStatuses) a los enums del Core.
/// Los números de los enums son iguales a los Id de las tablas (ver Core/Enums/Enums.cs).
/// </summary>
public class SqlStatusMapper
{
    public static OrderStatuses ToOrderStatus(int idStatus)
    {
        var status = (OrderStatuses)idStatus;
        return Enum.IsDefined(status)
            ? status
            : throw new InvalidOperationException($"El Id de estado de orden {idStatus} no existe en el enum OrderStatuses.");
    }

    public static ShipmentStatuses ToShipmentStatus(int idStatus)
    {
        var status = (ShipmentStatuses)idStatus;
        return Enum.IsDefined(status)
            ? status
            : throw new InvalidOperationException($"El Id de estado de envío {idStatus} no existe en el enum ShipmentStatuses.");
    }
}

public static class OrderSqlMapper
{
    /// <summary>
    /// Reconstruye una orden guardada. Las direcciones se reciben ya armadas porque
    /// la fila de la orden solo trae sus Id.
    /// </summary>
    public static Order ToEntity(OrderSqlRow row, Address origin, Address destination)
    => new(new SupplierRole(row.SupplierId), new CustomerRole(row.CustomerId), origin, destination)
    {
        Key = row.Code,
        CreatedAt = row.CreatedAt,
        Status = SqlStatusMapper.ToOrderStatus(row.IdStatus),
        Description = row.Description ?? ""
    };
}

public static class ShipmentSqlMapper
{
    /// <summary>Reconstruye un envío guardado (usa Shipment.Restore, sin validar fechas viejas).</summary>
    public static Shipment ToEntity(
        ShipmentSqlRow row, Driver? driver, Vehicle? vehicle, IEnumerable<Package>? packages = null)
    => Shipment.Restore(
        key: row.Code,
        orderKey: row.CodeOrder,
        status: SqlStatusMapper.ToShipmentStatus(row.IdStatus),
        driver: driver,
        vehicle: vehicle,
        description: row.Description,
        estimatedPickUpAt: row.EstimatedPickupAt,
        pickedUpAt: row.PickedUpAt,
        deliveredAt: row.DeliveredAt,
        packages: packages ?? Array.Empty<Package>());
}

public static class PackageSqlMapper
{
    public static Package ToEntity(PackageSqlRow row)
    => new(new Product(row.ProductCode, row.ProductName) { Key = row.IdProduct }, row.WeightKg, row.ProductAmount)
    {
        Key = row.Id,
        ShipmentCode = row.ShipmentCode
    };
}

public static class DriverSqlMapper
{
    public static Driver ToEntity(DriverSqlRow row)
    => new(row.Name, row.MiddleName, row.LastName, row.SecondLastName ?? "") { Key = row.Id, Code = row.Code, InUse = row.InUse };
}

public static class VehicleSqlMapper
{
    public static Vehicle ToEntity(VehicleSqlRow row)
    => new() { Key = row.Id, Plate = row.Plate, Description = row.Description ?? "", InUse = row.InUse };
}