using YHTransporte.Core.Enums;

namespace YHTransporte.Application.Shipments.Dtos;

/// <summary>
/// Una línea del historial de envíos de un vehículo o conductor (vista dbo.vw_ShipmentHistory).
/// </summary>
/// <param name="Origin">Municipio de origen de la orden.</param>
/// <param name="Destination">Municipio de destino de la orden.</param>
public sealed record ShipmentHistoryDto(
string ShipmentCode,
string OrderCode,
ShipmentStatuses Status,
string Origin,
string Destination,
DateTimeOffset? EstimatedPickUpAt,
DateTimeOffset? DeliveredAt);
