using System.Data;
using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.Data.SqlClient;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Enums;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Mappers;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders.Mappers;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders;

public sealed class SqlServerOrderRepository(DbConnectionFactory factory) : IOrderRepository
{
    private readonly DbConnectionFactory _factory = factory;

    // Código que tiene una orden/envío que todavía no se guardó (ver Order.Key y Shipment.Key en el Core).
    private const string UnsavedKey = "-1";

    private const string OrderColumns =
        "Code, CreatedAt, Description, IdStatus, SupplierId, CustomerId, IdOriginDir, IdDestinationDir";

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

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    => await AddAsync([entity], cancellationToken);

    public async Task AddAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        Order[] orders = [.. entities];

        if (orders.Length == 0)
            return;

        // Una sola transacción: si falla una orden (o uno de sus envíos), no se guarda ninguna.
        await InTransactionAsync(async (connection, transaction) =>
        {
            foreach (var order in orders)
                await InsertOrderAsync(connection, transaction, order, cancellationToken);
        }, cancellationToken);
    }



    public async Task<bool> Exists(string key, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Orders WHERE Code = @Key)
            THEN 1 ELSE 0 END AS BIT);
            """,
            new { Key = key },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IEnumerable<Order>> GetByKeysAsync(IEnumerable<string> key, CancellationToken cancellationToken = default)
    {
        string[] keys = [.. key];

        if (keys.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {OrderColumns} FROM dbo.vw_OrderDetails WHERE Code IN @Keys ORDER BY Code;",
            new { Keys = keys },
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<OrderSqlRow>(command)).ToList();

        return await BuildOrdersAsync(connection, rows, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetEverythingAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            $"SELECT {OrderColumns} FROM dbo.vw_OrderDetails ORDER BY Code;",
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<OrderSqlRow>(command)).ToList();

        return await BuildOrdersAsync(connection, rows, cancellationToken);
    }

    public async Task<string?> GetLastCode(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "GetLastOrderCode",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<string>(command);
    }






    // ============================================================
    // Helpers: guardar
    // ============================================================

    private static async Task InsertOrderAsync(
        SqlConnection connection, IDbTransaction transaction, Order order, CancellationToken cancellationToken)
    {
        // El estado de una orden NUEVA es siempre Pending; los cambios de estado se hacen con ChangeOrderStatus.
        if (order.Status != OrderStatuses.Pending)
            throw new InvalidOperationException(
                "Una orden nueva solo se puede guardar en estado Pending. Use el procedimiento ChangeOrderStatus para cambiarlo.");

        string code = order.Key != UnsavedKey
            ? order.Key
            : OrderCode.Generate(await GetLastCodeAsync(connection, transaction, LastOrderCodeLocked, cancellationToken));

        await connection.ExecuteAsync(Procedure(
            "InsertOrder",
            new
            {
                Code = code,
                order.SupplierId,
                order.CustomerId,
                IdOriginDir = order.OriginDirection.Key,
                IdDestinationDir = order.DestinationDirection.Key,
                Description = NullIfEmpty(order.Description),
                CreatedAt = order.CreatedAt
            },
            transaction, cancellationToken));

        foreach (var shipment in order.Shipments)
            await InsertShipmentAsync(connection, transaction, code, shipment, cancellationToken);
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

    private static async Task<string?> GetLastCodeAsync(
        SqlConnection connection, IDbTransaction transaction, string sql, CancellationToken cancellationToken)
    => await connection.QueryFirstOrDefaultAsync<string>(
        new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));

    private static CommandDefinition Procedure(
        string name, object parameters, IDbTransaction transaction, CancellationToken cancellationToken)
    => new(name, parameters, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

    /// <summary>Abre conexión + transacción, ejecuta el trabajo y confirma; si algo falla, deshace todo.</summary>
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

        private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    // En el Core ProductAmount es decimal, pero en SQL (ShipmentsDetails.ProductAmount) es INT.
    private static int ToWholeAmount(decimal amount)
    {
        if (amount < 0 || amount > int.MaxValue || amount != decimal.Truncate(amount))
            throw new InvalidOperationException(
                $"La cantidad de producto ({amount}) debe ser un número entero positivo para guardarse en la base de datos.");

        return (int)amount;
    }

    // ============================================================
    // Helpers: leer
    // ============================================================

    private static async Task<IEnumerable<Order>> BuildOrdersAsync(
        SqlConnection connection, List<OrderSqlRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return [];

        // Las filas de la orden solo traen el Id de sus direcciones; se piden todas juntas en una consulta.
        int[] addressIds = [.. rows.SelectMany(r => new[] { r.IdOriginDir, r.IdDestinationDir }).Distinct()];

        var command = new CommandDefinition(
            "SELECT * FROM dbo.AddressDetails WHERE Id IN @Ids;",
            new { Ids = addressIds },
            cancellationToken: cancellationToken);

        var addresses = (await connection.QueryAsync<AddressSqlDto>(command))
            .Select(AddressSqlMapper.ToEntity)
            .ToDictionary(a => a.Key);

        return [.. rows.Select(r => OrderSqlMapper.ToEntity(r, addresses[r.IdOriginDir], addresses[r.IdDestinationDir]))];
    }

    public Task ChangeOrderState(string orderCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CancelateOrder(string orderCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<OrderStatuses> GetOrderStatus(string orderCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}