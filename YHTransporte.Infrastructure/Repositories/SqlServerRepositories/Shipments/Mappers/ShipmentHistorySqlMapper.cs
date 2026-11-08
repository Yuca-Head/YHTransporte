using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Mappers;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shipments.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shipments.Mappers;

public static class ShipmentHistorySqlMapper
{
    public static ShipmentHistoryDto ToDto(ShipmentHistorySqlRow row)
    => new(row.ShipmentCode, row.OrderCode, SqlStatusMapper.ToShipmentStatus(row.IdStatus),
    row.Origin, row.Destination, row.EstimatedPickupAt, row.DeliveredAt);
}
