namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Reports.Entities;

// Entidades de EF Core con sus relaciones (propiedades de navegación).
// Reflejan las tablas que ya existen en SQL Server; no se usan migraciones.
// Son internal: solo las ve Infrastructure, el resto del sistema trabaja con las entidades del Core.

internal sealed class OrderStatusRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

internal sealed class ShipmentStatusRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

internal sealed class OrderRecord
{
    public string Code { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public int SupplierId { get; set; }
    public int CustomerId { get; set; }
    public int IdOriginDir { get; set; }
    public int IdDestinationDir { get; set; }
    public int IdStatus { get; set; }
    public string? Description { get; set; }

    public OrderStatusRecord Status { get; set; } = null!;
    public List<ShipmentRecord> Shipments { get; set; } = [];
}

internal sealed class ShipmentRecord
{
    public string Code { get; set; } = "";
    public string CodeOrder { get; set; } = "";
    public int? IdVehicle { get; set; }
    public int? IdDriver { get; set; }
    public DateTimeOffset? EstimatedPickupAt { get; set; }
    public DateTimeOffset? PickedUpAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public int IdStatus { get; set; }
    public string? Description { get; set; }

    public OrderRecord Order { get; set; } = null!;
    public ShipmentStatusRecord Status { get; set; } = null!;
    public List<PackageRecord> Packages { get; set; } = [];
}

/// <summary>Un paquete = una fila de ShipmentsDetails.</summary>
internal sealed class PackageRecord
{
    public string ShipmentCode { get; set; } = "";
    public int Id { get; set; }
    public int IdProduct { get; set; }
    public decimal WeightKg { get; set; }
    public int ProductAmount { get; set; }

    public ShipmentRecord Shipment { get; set; } = null!;
}
