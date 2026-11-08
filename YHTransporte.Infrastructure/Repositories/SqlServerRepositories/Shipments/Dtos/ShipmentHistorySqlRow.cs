namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shipments.Dtos;

/// <summary>Fila de dbo.vw_ShipmentHistory (los nombres deben ser iguales a las columnas de la vista).</summary>
public sealed record ShipmentHistorySqlRow(
    string ShipmentCode,
    string OrderCode,
    int IdStatus,
    string Origin,
    string Destination,
    DateTimeOffset? EstimatedPickupAt,
    DateTimeOffset? DeliveredAt);
