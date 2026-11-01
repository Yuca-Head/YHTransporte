using Microsoft.EntityFrameworkCore;
using YHTransporte.Application.Reports;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Reports;

/// <summary>
/// Reporte de envíos con Entity Framework Core y LINQ.
/// Usa IDbContextFactory porque el repositorio es Singleton (un DbContext por consulta).
/// </summary>
public sealed class EfShipmentReportRepository(IDbContextFactory<YHTransporteDbContext> contextFactory)
    : IShipmentReportRepository
{
    private readonly IDbContextFactory<YHTransporteDbContext> _contextFactory = contextFactory;

    public async Task<ShipmentReport> GetShipmentReportAsync(
        ShipmentReportFilter filter, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // 1) Filtrar: la consulta se va armando y recién se ejecuta al final (ToListAsync).
        IQueryable<Entities.ShipmentRecord> shipments = context.Shipments.AsNoTracking();

        if (filter.From is DateTimeOffset from)
            shipments = shipments.Where(s => s.Order.CreatedAt >= from);

        if (filter.To is DateTimeOffset to)
            shipments = shipments.Where(s => s.Order.CreatedAt <= to);

        if (filter.ShipmentStatusId is int statusId)
            shipments = shipments.Where(s => s.IdStatus == statusId);

        if (filter.SupplierId is int supplierId)
            shipments = shipments.Where(s => s.Order.SupplierId == supplierId);

        // 2) Unir con los nombres de proveedor y cliente (ambos son filas de ThirdParties),
        //    ordenar (más recientes primero) y proyectar solo lo que muestra el reporte.
        var rows = await (
            from s in shipments
            join supplier in context.ThirdParties on s.Order.SupplierId equals supplier.Id
            join customer in context.ThirdParties on s.Order.CustomerId equals customer.Id
            orderby s.Order.CreatedAt descending, s.Code
            select new ShipmentReportRow(
                s.Code,
                s.CodeOrder,
                supplier.Name,
                customer.Name,
                s.Status.Name,
                s.Order.CreatedAt,
                s.EstimatedPickupAt,
                s.DeliveredAt,
                s.Packages.Count(),
                s.Packages.Sum(p => (decimal?)p.WeightKg) ?? 0m))
            .ToListAsync(cancellationToken);

        // 3) Totales por estado, agrupando en memoria las filas ya traídas.
        var byStatus = rows
            .GroupBy(r => r.Status)
            .Select(g => new ShipmentStatusSummary(
                g.Key, g.Count(), g.Sum(r => r.PackageCount), g.Sum(r => r.TotalWeightKg)))
            .OrderBy(x => x.Status)
            .ToList();

        return new ShipmentReport(rows, byStatus, DateTimeOffset.Now);
    }

    public async Task<IReadOnlyList<ReportOption>> GetShipmentStatusOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.ShipmentStatuses
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new ReportOption(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReportOption>> GetSupplierOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.ThirdParties
            .AsNoTracking()
            .Where(x => x.IsSupplier)
            .OrderBy(x => x.Name)
            .Select(x => new ReportOption(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }
}
