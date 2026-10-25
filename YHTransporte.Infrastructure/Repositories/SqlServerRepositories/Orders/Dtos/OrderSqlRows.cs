namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Dtos;

// Cada "Row" es una fila tal cual sale de SQL Server. Los nombres de los parámetros del constructor
// deben ser IGUALES a los nombres de las columnas del SELECT: así es como Dapper las asocia.

/// <summary>Fila de dbo.vw_OrderDetails (solo las columnas que necesita la entidad Order).</summary>
public sealed record OrderSqlRow(
    string Code,
    DateTimeOffset CreatedAt,
    string? Description,
    int IdStatus,
    int SupplierId,
    int CustomerId,
    int IdOriginDir,
    int IdDestinationDir);

/// <summary>Fila de dbo.vw_ShipmentDetails (solo las columnas que necesita la entidad Shipment).</summary>
public sealed record ShipmentSqlRow(
    string Code,
    string CodeOrder,
    string? Description,
    int IdStatus,
    int? IdVehicle,
    int? IdDriver,
    DateTimeOffset? EstimatedPickupAt,
    DateTimeOffset? PickedUpAt,
    DateTimeOffset? DeliveredAt);

/// <summary>Fila de dbo.vw_PackageDetails.</summary>
public sealed record PackageSqlRow(
    int Id,
    int IdProduct,
    string ProductCode,
    string ProductName,
    decimal WeightKg,
    int ProductAmount);

/// <summary>Fila de dbo.Drivers.</summary>
public sealed record DriverSqlRow(
    int Id,
    string Code,
    string Name,
    string? MiddleName,
    string LastName,
    string? SecondLastName);

/// <summary>Fila de dbo.Vehicles.</summary>
public sealed record VehicleSqlRow(
    int Id,
    string Plate,
    string? Description);
