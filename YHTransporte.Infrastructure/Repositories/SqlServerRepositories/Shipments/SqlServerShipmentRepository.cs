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

    public async Task<string?> GetLastShipmentCode(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "GetLastShipmentCode",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<string>(command);
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
            """
            SELECT Id, IdProduct, ProductCode, ProductName, WeightKg, ProductAmount
            FROM dbo.vw_PackageDetails
            WHERE ShipmentCode = @ShipmentId
            ORDER BY Id;
            """,
            new { ShipmentId = shipmentId },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<PackageSqlRow>(command);

        foreach (var row in rows)
            yield return PackageSqlMapper.ToEntity(row);
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
                "SELECT Id, Code, Name, MiddleName, LastName, SecondLastName FROM dbo.Drivers WHERE Id IN @Ids;",
                new { Ids = driverIds },
                cancellationToken: cancellationToken);

            drivers = (await connection.QueryAsync<DriverSqlRow>(command))
                .Select(DriverSqlMapper.ToEntity)
                .ToDictionary(d => d.Key);
        }

        if (vehicleIds.Length > 0)
        {
            var command = new CommandDefinition(
                "SELECT Id, Plate, Description FROM dbo.Vehicles WHERE Id IN @Ids;",
                new { Ids = vehicleIds },
                cancellationToken: cancellationToken);

            vehicles = (await connection.QueryAsync<VehicleSqlRow>(command))
                .Select(VehicleSqlMapper.ToEntity)
                .ToDictionary(v => v.Key);
        }

        return (drivers, vehicles);
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

        foreach (var package in shipment.Packages)
            await InsertPackageAsync(connection, transaction, code, package, cancellationToken);
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
    => await connection.QueryFirstOrDefaultAsync<string>(
        new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));



    public async Task AddAsync(Shipment entity, CancellationToken cancellationToken = default)
    {
        var orderCode = entity.OrderKey;
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);
        ArgumentNullException.ThrowIfNull(entity);

        await InTransactionAsync(async (connection, transaction) =>
        {
            await InsertShipmentAsync(connection, transaction, orderCode, entity, cancellationToken);
        }, cancellationToken);
    }

//Handle all these cases
    public Task AddAsync(IEnumerable<Shipment> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Shipment>> GetByKeysAsync(IEnumerable<string> key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Shipment>> GetEverythingAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Exists(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ChangeState(string shipmentCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ShipmentStatuses> GetShipmentStatus(string code, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CancelateShipment(string shipmentCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}