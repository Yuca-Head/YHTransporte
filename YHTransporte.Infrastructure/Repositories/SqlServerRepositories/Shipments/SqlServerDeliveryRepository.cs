using System.Data;
using System.Runtime.CompilerServices;
using Dapper;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Mappers;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shipments;

public sealed class SqlServerDeliveryRepository(DbConnectionFactory factory) : IDeliveryRepository
{
    private readonly DbConnectionFactory _factory = factory;

    private const string DriverColumns = "Id, Code, Name, MiddleName, LastName, SecondLastName, InUse";
    private const string VehicleColumns = "Id, Plate, Description, InUse";

    // ============================================================
    // Guardar
    // ============================================================

    public async Task AddVehicle(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "InsertVehicle",
            new { vehicle.Plate, Description = NullIfEmpty(vehicle.Description) },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task AddDriver(Driver driver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(driver);

        using var connection = _factory.Create();

        // El código no se envía: lo genera InsertDriver.
        var command = new CommandDefinition(
            "InsertDriver",
            new
            {
                driver.Name,
                MiddleName = NullIfEmpty(driver.MiddleName),
                driver.LastName,
                SecondLastName = NullIfEmpty(driver.SecondLastName)
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    // ============================================================
    // Leer
    // ============================================================

    public async IAsyncEnumerable<Driver> GetDrivers(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {DriverColumns} FROM dbo.Drivers ORDER BY Id;",
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<DriverSqlRow>(command);

        foreach (var row in rows)
            yield return DriverSqlMapper.ToEntity(row);
    }

    public async IAsyncEnumerable<Vehicle> GetVehicles(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {VehicleColumns} FROM dbo.Vehicles ORDER BY Id;",
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<VehicleSqlRow>(command);

        foreach (var row in rows)
            yield return VehicleSqlMapper.ToEntity(row);
    }

    public async Task<Driver?> GetDriverAsync(string driverCode, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {DriverColumns} FROM dbo.Drivers WHERE Code = @Code;",
            new { Code = driverCode.Trim() },
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<DriverSqlRow>(command);

        return row is null ? null : DriverSqlMapper.ToEntity(row);
    }

    public async Task<Vehicle?> GetVehicleAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {VehicleColumns} FROM dbo.Vehicles WHERE Id = @Id;",
            new { Id = vehicleId },
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<VehicleSqlRow>(command);

        return row is null ? null : VehicleSqlMapper.ToEntity(row);
    }

    public async Task<bool> VehicleExists(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Id = @Id)
            THEN 1 ELSE 0 END AS BIT);
            """,
            new { Id = vehicleId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> DriverExists(string driverCode, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Drivers WHERE Code = @Code)
            THEN 1 ELSE 0 END AS BIT);
            """,
            new { Code = driverCode.Trim() },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    // ============================================================
    // Estado "In Use"
    // ============================================================

    // Cambia InUse al valor contrario. Normalmente lo hacen los procedimientos de envíos
    // (se ocupa al crear el envío y se libera al entregarlo o cancelarlo).
    public async Task ChangeDriverState(string driverCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driverCode);

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "UPDATE dbo.Drivers SET InUse = CASE WHEN InUse = 1 THEN 0 ELSE 1 END WHERE Code = @Code;",
            new { Code = driverCode.Trim() },
            cancellationToken: cancellationToken);

        if (await connection.ExecuteAsync(command) == 0)
            throw new KeyNotFoundException($"No existe el conductor con código {driverCode}.");
    }

    public async Task ChangeVehicleState(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "UPDATE dbo.Vehicles SET InUse = CASE WHEN InUse = 1 THEN 0 ELSE 1 END WHERE Id = @Id;",
            new { Id = vehicleId },
            cancellationToken: cancellationToken);

        if (await connection.ExecuteAsync(command) == 0)
            throw new KeyNotFoundException($"No existe el vehículo con Id {vehicleId}.");
    }

    private static string? NullIfEmpty(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value;
}