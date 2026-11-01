namespace YHTransporte.Application.Reports;

public interface IShipmentReportRepository
{
    /// <summary>Genera el reporte de envíos. Se puede cancelar con el token (pensado para correr en segundo plano).</summary>
    Task<ShipmentReport> GetShipmentReportAsync(ShipmentReportFilter filter, CancellationToken cancellationToken = default);

    /// <summary>Estados de envío disponibles para el filtro.</summary>
    Task<IReadOnlyList<ReportOption>> GetShipmentStatusOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Proveedores disponibles para el filtro.</summary>
    Task<IReadOnlyList<ReportOption>> GetSupplierOptionsAsync(CancellationToken cancellationToken = default);
}
