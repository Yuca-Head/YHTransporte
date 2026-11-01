namespace YHTransporte.Application.Reports;

/// <summary>Filtros del reporte. Todos son opcionales; sin filtros se trae todo.</summary>
/// <param name="From">Órdenes creadas desde esta fecha (inclusive).</param>
/// <param name="To">Órdenes creadas hasta esta fecha (inclusive).</param>
/// <param name="ShipmentStatusId">Id de ShipmentStatuses (1 Pending, 2 On way, 3 Delivered, 4 Canceled).</param>
/// <param name="SupplierId">Id del tercero proveedor de la orden.</param>
public readonly record struct ShipmentReportFilter(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int? ShipmentStatusId = null,
    int? SupplierId = null);

/// <summary>Una fila del reporte: un envío con los datos de su orden y de su carga.</summary>
public sealed record ShipmentReportRow(
    string ShipmentCode,
    string OrderCode,
    string SupplierName,
    string CustomerName,
    string Status,
    DateTimeOffset OrderCreatedAt,
    DateTimeOffset? EstimatedPickupAt,
    DateTimeOffset? DeliveredAt,
    int PackageCount,
    decimal TotalWeightKg);

/// <summary>Totales por estado (el pie del reporte).</summary>
public sealed record ShipmentStatusSummary(string Status, int Shipments, int Packages, decimal TotalWeightKg);

public sealed record ShipmentReport(
    IReadOnlyList<ShipmentReportRow> Rows,
    IReadOnlyList<ShipmentStatusSummary> ByStatus,
    DateTimeOffset GeneratedAt)
{
    public int TotalShipments => Rows.Count;
    public decimal TotalWeightKg => Rows.Sum(r => r.TotalWeightKg);
}

/// <summary>Opción para llenar un ComboBox de filtro (estado o proveedor).</summary>
public sealed record ReportOption(int Id, string Name);
