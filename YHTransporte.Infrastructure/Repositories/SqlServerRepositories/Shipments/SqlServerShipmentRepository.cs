using System.Data;
using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.Data.SqlClient;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Mappers;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shipments;

public sealed class SqlServerShipmentRepository(DbConnectionFactory factory) : IShipmentRepository
{

    private readonly DbConnectionFactory _factory = factory;

    private const string UnsavedKey = "-1";
    private const string ShipmentColumns =
        "Code, CodeOrder, Description, IdStatus, IdVehicle, IdDriver, EstimatedPickupAt, PickedUpAt, DeliveredAt";
    private const string PackageColumns =
        "ShipmentCode, Id, IdProduct, ProductCode, ProductName, WeightKg, ProductAmount";
    private const string LastOrderCodeLocked =
    """
    SELECT TOP 1 Code FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK)
    ORDER BY TRY_CAST(LEFT(Code, 4) AS INT) DESC;
    """;
    private const string LastShipmentCodeLocked =
    """
    SELECT TOP 1 Code FROM dbo.Shipments WITH (UPDLOCK, HOLDLOCK)
    ORDER BY TRY_CAST(LEFT(Code, 4) AS INT) DESC;
    """;

    // Conductor y vehículo de un envío: se bloquea la fila hasta terminar la transacción para que
    // dos envíos creados a la vez no puedan tomar el mismo conductor o el mismo vehículo.
    private const string VehicleInUseLocked = "SELECT InUse FROM dbo.Vehicles WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id;";
    private const string DriverInUseLocked = "SELECT InUse FROM dbo.Drivers WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id;";
    private const string SetVehicleInUse = "UPDATE dbo.Vehicles SET InUse = 1 WHERE Id = @Id;";
    private const string SetDriverInUse = "UPDATE dbo.Drivers SET InUse = 1 WHERE Id = @Id;";

    // ============================================================
    // Leer
    // ============================================================

    public async Task<string?> GetLastShipmentCode(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "GetLastShipmentCode",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<string>(command);
    }

    public async Task<bool> Exists(string key, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Shipments WHERE Code = @Key)
            THEN 1 ELSE 0 END AS BIT);
            """,
            new { Key = key },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IEnumerable<Shipment>> GetByKeysAsync(IEnumerable<string> key, CancellationToken cancellationToken = default)
    {
        string[] keys = [.. key];

        if (keys.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {ShipmentColumns} FROM dbo.vw_ShipmentDetails WHERE Code IN @Keys ORDER BY Code;",
            new { Keys = keys },
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<ShipmentSqlRow>(command)).ToList();

        return await BuildShipmentsAsync(connection, rows, cancellationToken);
    }

    public async Task<IEnumerable<Shipment>> GetEverythingAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {ShipmentColumns} FROM dbo.vw_ShipmentDetails ORDER BY Code;",
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<ShipmentSqlRow>(command)).ToList();

        return await BuildShipmentsAsync(connection, rows, cancellationToken);
    }

    public async Task<ShipmentStatuses> GetShipmentStatus(string code, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT IdStatus FROM dbo.Shipments WHERE Code = @Code;",
            new { Code = code },
            cancellationToken: cancellationToken);

        int? idStatus = await connection.QuerySingleOrDefaultAsync<int?>(command);

        return idStatus is int id
            ? SqlStatusMapper.ToShipmentStatus(id)
            : throw new KeyNotFoundException($"No existe el envío con código {code}.");
    }

    public async IAsyncEnumerable<Shipment> GetOrderShipments(
        string orderId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {ShipmentColumns} FROM dbo.vw_ShipmentDetails WHERE CodeOrder = @OrderId ORDER BY Code;",
            new { OrderId = orderId },
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<ShipmentSqlRow>(command)).ToList();

        var (drivers, vehicles) = await LoadDriversAndVehiclesAsync(connection, rows, cancellationToken);

        foreach (var row in rows)
        {
            Driver? driver = row.IdDriver is int driverId && drivers.TryGetValue(driverId, out var d) ? d : null;
            Vehicle? vehicle = row.IdVehicle is int vehicleId && vehicles.TryGetValue(vehicleId, out var v) ? v : null;

            yield return ShipmentSqlMapper.ToEntity(row, driver, vehicle);
        }
    }


    public async IAsyncEnumerable<Package> GetShipmentPackages(
        string shipmentId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {PackageColumns} FROM dbo.vw_PackageDetails WHERE ShipmentCode = @ShipmentId ORDER BY Id;",
            new { ShipmentId = shipmentId },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<PackageSqlRow>(command);

        foreach (var row in rows)
            yield return PackageSqlMapper.ToEntity(row);
    }

    /// <summary>Arma los envíos con su conductor, su vehículo y sus paquetes (un envío puede no tener ninguno de los dos primeros).</summary>
    private static async Task<IEnumerable<Shipment>> BuildShipmentsAsync(
        SqlConnection connection, List<ShipmentSqlRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return [];

        var (drivers, vehicles) = await LoadDriversAndVehiclesAsync(connection, rows, cancellationToken);
        var packages = await LoadPackagesAsync(connection, [.. rows.Select(r => r.Code)], cancellationToken);

        return [.. rows.Select(row =>
        {
            Driver? driver = row.IdDriver is int driverId && drivers.TryGetValue(driverId, out var d) ? d : null;
            Vehicle? vehicle = row.IdVehicle is int vehicleId && vehicles.TryGetValue(vehicleId, out var v) ? v : null;

            return ShipmentSqlMapper.ToEntity(row, driver, vehicle, packages[row.Code]);
        })];
    }

    private static async Task<ILookup<string, Package>> LoadPackagesAsync(
        SqlConnection connection, string[] shipmentCodes, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            $"SELECT {PackageColumns} FROM dbo.vw_PackageDetails WHERE ShipmentCode IN @Codes ORDER BY ShipmentCode, Id;",
            new { Codes = shipmentCodes },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<PackageSqlRow>(command);

        return rows.ToLookup(r => r.ShipmentCode, PackageSqlMapper.ToEntity);
    }


    private static async Task<(Dictionary<int, Driver> Drivers, Dictionary<int, Vehicle> Vehicles)>
    LoadDriversAndVehiclesAsync(
        SqlConnection connection, List<ShipmentSqlRow> rows, CancellationToken cancellationToken)
    {
        int[] driverIds = [.. rows.Where(r => r.IdDriver.HasValue).Select(r => r.IdDriver!.Value).Distinct()];
        int[] vehicleIds = [.. rows.Where(r => r.IdVehicle.HasValue).Select(r => r.IdVehicle!.Value).Distinct()];

        Dictionary<int, Driver> drivers = [];
        Dictionary<int, Vehicle> vehicles = [];

        if (driverIds.Length > 0)
        {
            var command = new CommandDefinition(
                "SELECT Id, Code, Name, MiddleName, LastName, SecondLastName, InUse FROM dbo.Drivers WHERE Id IN @Ids;",
                new { Ids = driverIds },
                cancellationToken: cancellationToken);

            drivers = (await connection.QueryAsync<DriverSqlRow>(command))
                .Select(DriverSqlMapper.ToEntity)
                .ToDictionary(d => d.Key);
        }

        if (vehicleIds.Length > 0)
        {
            var command = new CommandDefinition(
                "SELECT Id, Plate, Description, InUse FROM dbo.Vehicles WHERE Id IN @Ids;",
                new { Ids = vehicleIds },
                cancellationToken: cancellationToken);

            vehicles = (await connection.QueryAsync<VehicleSqlRow>(command))
                .Select(VehicleSqlMapper.ToEntity)
                .ToDictionary(v => v.Key);
        }

        return (drivers, vehicles);
    }

    // ============================================================
    // Guardar
    // ============================================================

    public async Task AddAsync(Shipment entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await AddAsync([entity], cancellationToken);
    }

    public async Task AddAsync(IEnumerable<Shipment> entities, CancellationToken cancellationToken = default)
    {
        Shipment[] shipments = [.. entities];

        if (shipments.Length == 0)
            return;

        foreach (var shipment in shipments)
        {
            ArgumentNullException.ThrowIfNull(shipment);
            ArgumentException.ThrowIfNullOrWhiteSpace(shipment.OrderKey);
        }

        // Una sola transacción: si falla un envío, no se guarda ninguno (ni se cambia el InUse de nadie).
        await InTransactionAsync(async (connection, transaction) =>
        {
            foreach (var shipment in shipments)
                await InsertShipmentAsync(connection, transaction, shipment.OrderKey, shipment, cancellationToken);
        }, cancellationToken);
    }

    public async Task AddPackage(string shipmentCode, Package package, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shipmentCode);
        ArgumentNullException.ThrowIfNull(package);

        await InTransactionAsync(async (connection, transaction) =>
        {
            await InsertPackageAsync(connection, transaction, shipmentCode, package, cancellationToken);
        }, cancellationToken);
    }

    // ============================================================
    // Cambios de estado
    // ============================================================

    /// <summary>Pasa el envío al siguiente estado: Pending -> OnWay -> Delivered. Las reglas las valida ChangeShipmentStatus.</summary>
    public async Task ChangeState(string shipmentCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shipmentCode);

        var current = await GetShipmentStatus(shipmentCode, cancellationToken);

        ShipmentStatuses next = current switch
        {
            ShipmentStatuses.Pending => ShipmentStatuses.OnWay,
            ShipmentStatuses.OnWay => ShipmentStatuses.Delivered,
            _ => throw new InvalidOperationException($"Un envío en estado {current} ya no puede cambiar de estado.")
        };

        await ChangeStatusAsync(shipmentCode, next, cancellationToken);
    }

    public async Task CancelateShipment(string shipmentCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shipmentCode);

        await ChangeStatusAsync(shipmentCode, ShipmentStatuses.Canceled, cancellationToken);
    }

    private async Task ChangeStatusAsync(string shipmentCode, ShipmentStatuses status, CancellationToken cancellationToken)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "ChangeShipmentStatus",
            new { Code = shipmentCode, IdStatus = (int)status },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    // ============================================================
    // Helpers: guardar
    // ============================================================

    private async Task InTransactionAsync(
        Func<SqlConnection, IDbTransaction, Task> work, CancellationToken cancellationToken)
    {
        using SqlConnection connection = _factory.Create();
        await connection.OpenAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            await work(connection, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    private static async Task InsertShipmentAsync(
        SqlConnection connection, IDbTransaction transaction, string orderCode, Shipment shipment,
        CancellationToken cancellationToken)
    {
        // Igual que con la orden: un envío nuevo nace Pending (el cambio lo hace ChangeShipmentStatus).
        if (shipment.Status != ShipmentStatuses.Pending)
            throw new InvalidOperationException(
                "Un envío nuevo solo se puede guardar en estado Pending. Use el procedimiento ChangeShipmentStatus para cambiarlo.");

        // Se comprueba antes de guardar que el vehículo y el conductor estén libres.
        if (shipment.Vehicle is not null)
            await EnsureIsFreeAsync(connection, transaction, VehicleInUseLocked, shipment.Vehicle.Key, "El vehículo", cancellationToken);

        if (shipment.Driver is not null)
            await EnsureIsFreeAsync(connection, transaction, DriverInUseLocked, shipment.Driver.Key, "El conductor", cancellationToken);

        string code = shipment.Key != UnsavedKey
            ? shipment.Key
            : OrderCode.Generate(await GetLastCodeAsync(connection, transaction, LastShipmentCodeLocked, cancellationToken));

        await connection.ExecuteAsync(Procedure(
            "InsertShipment",
            new
            {
                Code = code,
                CodeOrder = orderCode,
                IdVehicle = shipment.Vehicle?.Key,
                IdDriver = shipment.Driver?.Key,
                EstimatedPickupAt = shipment.EstimatedPickUpAt,
                Description = NullIfEmpty(shipment.Description)
            },
            transaction, cancellationToken));

        // Al crear el envío, el vehículo y el conductor pasan a "In Use".
        if (shipment.Vehicle is not null)
            await connection.ExecuteAsync(new CommandDefinition(
                SetVehicleInUse, new { Id = shipment.Vehicle.Key }, transaction, cancellationToken: cancellationToken));

        if (shipment.Driver is not null)
            await connection.ExecuteAsync(new CommandDefinition(
                SetDriverInUse, new { Id = shipment.Driver.Key }, transaction, cancellationToken: cancellationToken));

        foreach (var package in shipment.Packages)
            await InsertPackageAsync(connection, transaction, code, package, cancellationToken);
    }

    private static async Task EnsureIsFreeAsync(
        SqlConnection connection, IDbTransaction transaction, string lockSql, int id, string name,
        CancellationToken cancellationToken)
    {
        bool? inUse = await connection.QuerySingleOrDefaultAsync<bool?>(new CommandDefinition(
            lockSql, new { Id = id }, transaction, cancellationToken: cancellationToken));

        if (inUse is null)
            throw new InvalidOperationException($"{name} con Id {id} no existe.");

        if (inUse.Value)
            throw new InvalidOperationException($"{name} con Id {id} ya se encuentra en uso.");
    }

    private static async Task InsertPackageAsync(
        SqlConnection connection, IDbTransaction transaction, string shipmentCode, Package package,
        CancellationToken cancellationToken)
    {
        // El Id del paquete (ShipmentsDetails.Id) lo calcula el procedimiento, por eso no se envía.
        await connection.ExecuteAsync(Procedure(
            "InsertShipmentsDetail",
            new
            {
                Code = shipmentCode,
                Product = package.Product.Key,
                Kg = package.Weight,
                Amount = ToWholeAmount(package.ProductAmount)
            },
            transaction, cancellationToken));
    }

    private static CommandDefinition Procedure(
    string name, object parameters, IDbTransaction transaction, CancellationToken cancellationToken)
    => new(name, parameters, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

    private static int ToWholeAmount(decimal amount)
    {
        if (amount < 0 || amount > int.MaxValue || amount != decimal.Truncate(amount))
            throw new InvalidOperationException(
                $"La cantidad de producto ({amount}) debe ser un número entero positivo para guardarse en la base de datos.");

        return (int)amount;
    }

        private static string? NullIfEmpty(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value;

    private static async Task<string?> GetLastCodeAsync(
        SqlConnection connection, IDbTransaction transaction, string sql, CancellationToken cancellationToken)
    { 

        var utcDate = DateTimeOffset.UtcNow;
        var todayPattern = $"____-{utcDate:ddMMyy}%";

        return await connection.QueryFirstOrDefaultAsync<string>(
        new CommandDefinition(
            sql,
            new { TodayPattern = todayPattern },
            transaction: transaction,
            cancellationToken: cancellationToken));
    }
}