using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.EntityFrameworkCore;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Mappers;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties;

public sealed class SqlServerThirdPartyRepository(
    DbConnectionFactory factory,
    IDbContextFactory<YHTransporteDbContext> contextFactory) : IThirdPartyRepository
{
    private readonly DbConnectionFactory _factory = factory;
    private readonly IDbContextFactory<YHTransporteDbContext> _contextFactory = contextFactory;

  
    // Guardar (Entity Framework)
    

    public async Task AddAsync(ThirdParty entity, CancellationToken cancellationToken = default)
    => await AddAsync([entity], cancellationToken);

    public async Task AddAsync(IEnumerable<ThirdParty> entities, CancellationToken cancellationToken = default)
    {
       
        ThirdPartySqlDto[] dtos = [.. entities.Select(ThirdPartySqlMapper.ToValue)];

        if (dtos.Length == 0)
            return;

       
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.ThirdParties.AddRange(dtos);
        await context.SaveChangesAsync(cancellationToken);
    }

  
    // Leer (Dapper)
    

    public async Task<bool> Exists(int key)
    => await Exists(key, CancellationToken.None);

    public async Task<bool> Exists(int key, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.ThirdParties WHERE Id = @Key)
                             THEN 1 ELSE 0 END AS BIT);
            """,
            new { Key = key },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> NameExists(string name, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.ThirdParties WHERE Name = @Name)
                             THEN 1 ELSE 0 END AS BIT);
            """,
            new { Name = name.Trim() },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IEnumerable<string>> FindExistingNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        string[] allNames = [.. names.Select(x => x.Trim())];

        if (allNames.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT Name FROM dbo.ThirdParties WHERE Name IN @Names;",
            new { Names = allNames },
            cancellationToken: cancellationToken);

        var existing = await connection.QueryAsync<string>(command);

        return existing.ToList();
    }

    public async Task<IEnumerable<ThirdParty>> GetByKeysAsync(IEnumerable<int> keys, CancellationToken cancellationToken = default)
    {
        int[] allKeys = [.. keys];

        if (allKeys.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT
                Id,
                Name,
                IsSupplier,
                IsCustomer
            FROM dbo.ThirdParties
            WHERE Id IN @Keys;
            """,
            new { Keys = allKeys },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ThirdPartySqlDto>(command);

        return rows.Select(ThirdPartySqlMapper.ToEntity).ToList();
    }

    public async Task<IEnumerable<ThirdParty>> GetManyByKeysAsync(IEnumerable<int> keys, CancellationToken cancellationToken = default)
    => await GetByKeysAsync(keys, cancellationToken);

    public async Task<IEnumerable<ThirdParty>> GetEverythingAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT Id, Name, IsSupplier, IsCustomer FROM dbo.ThirdParties;",
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ThirdPartySqlDto>(command);

        return [.. rows.Select(ThirdPartySqlMapper.ToEntity)];
    }

    public async Task<IEnumerable<ThirdParty>?> TakeManyAsync(int take, CancellationToken cancellationToken = default)
    {
        if (take <= 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT TOP (@Take)
                Id,
                Name,
                IsSupplier,
                IsCustomer
            FROM dbo.ThirdParties
            ORDER BY Id;
            """,
            new { Take = take },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ThirdPartySqlDto>(command);

        return [.. rows.Select(ThirdPartySqlMapper.ToEntity)];
    }
    
    public async Task AddAddressToThirdParty(int addressId, int thirdPartyId, CancellationToken cancellationToken)
    {
       
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.ThirdPartyAddressesTable.Add(new(thirdPartyId, addressId));
        await context.SaveChangesAsync(cancellationToken);

    }

    // Productos

    public async IAsyncEnumerable<Product> GetSupplierProducts(
        int supplierId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT p.Id, p.Code, p.Name
            FROM dbo.SupplierProducts sp
            INNER JOIN dbo.Products p ON p.Id = sp.IdProduct
            WHERE sp.IdSupplier = @SupplierId
            ORDER BY p.Id;
            """,
            new { SupplierId = supplierId },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ProductSqlRow>(command);

        foreach (var row in rows)
            yield return ProductSqlMapper.ToEntity(row);
    }

    public async Task<Product?> GetProductByCode(string code, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT Id, Code, Name FROM dbo.Products WHERE Code = @Code;",
            new { Code = code.Trim() },
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<ProductSqlRow>(command);

        return row is null ? null : ProductSqlMapper.ToEntity(row);
    }

    public async Task<Product> AddProduct(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        var row = ProductSqlMapper.ToValue(product);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Products.Add(row);
        await context.SaveChangesAsync(cancellationToken);

        // Después de guardar, EF deja en la fila el Id que asignó la base de datos.
        return ProductSqlMapper.ToEntity(row);
    }

    public async Task AddProductToSupplier(int productId, int supplierId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.SupplierProducts.Add(new(productId, supplierId));
        await context.SaveChangesAsync(cancellationToken);
    }

    // Entity Framework: una sola sentencia UPDATE, sin cargar la fila.
    public async Task UpdateProduct(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        int affected = await context.Products
            .Where(x => x.Id == product.Key)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Code, product.Code)
                .SetProperty(x => x.Name, product.Name), cancellationToken);

        if (affected == 0)
            throw new KeyNotFoundException($"No existe el producto con Id {product.Key}.");
    }
}
