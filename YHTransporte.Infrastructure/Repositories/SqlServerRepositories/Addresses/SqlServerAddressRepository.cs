using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Mappers;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
using YHTransporte.Core.Exceptions;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses;

public sealed class SqlServerAddressRepository(
    DbConnectionFactory factory,
    IDbContextFactory<YHTransporteDbContext> contextFactory) : IAddressRepository
{
    private readonly DbConnectionFactory _factory = factory;
    private readonly IDbContextFactory<YHTransporteDbContext> _contextFactory = contextFactory;

    // ============================================================
    // Addresses
    // ============================================================

    public async Task AddAsync(Address entity, CancellationToken cancellationToken = default)
    => await AddAsync([entity], cancellationToken);

    public async Task AddAsync(IEnumerable<Address> entities, CancellationToken cancellationToken = default)
    {
        // Se guarda usando el Dto (no la entidad directamente)
        AddressSqlDto[] dtos = [.. entities.Select(AddressSqlMapper.ToValue)];

        if (dtos.Length == 0)
            return;

        await ExecuteInTransactionAsync(
            "InsertAddress",
            dtos.Select(x => new { x.Details, IdMunicipality = x.MunicipalityId }),
            cancellationToken);
    }

    public async Task<bool> Exists(int key, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Addresses WHERE Id = @Key)
                             THEN 1 ELSE 0 END AS BIT);
            """,
            new { Key = key },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IEnumerable<Address>> GetEverythingAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT * FROM dbo.AddressDetails;",
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<AddressSqlDto>(command);

        return rows.Select(AddressSqlMapper.ToEntity).ToList();
    }

    public async Task<IEnumerable<Address>> GetByKeysAsync(IEnumerable<int> key, CancellationToken cancellationToken = default)
    {
        int[] keys = [.. key];

        if (keys.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT * FROM dbo.AddressDetails WHERE Id IN @Keys;",
            new { Keys = keys },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<AddressSqlDto>(command);

        return rows.Select(AddressSqlMapper.ToEntity);
    }

    public async Task<IEnumerable<Address>> GetAddressesByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        string[] allNames = [.. names];

        if (allNames.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT * FROM dbo.AddressDetails WHERE Details IN @Names;",
            new { Names = allNames },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<AddressSqlDto>(command);

        return rows.Select(AddressSqlMapper.ToEntity);
    }

    public async Task<IEnumerable<(int ThirdPartyId, IEnumerable<Address> Addresses)>>
    GetAddressesFromThirdParties(
        IEnumerable<int> thirdPartyIds,
        CancellationToken cancellationToken = default)
    {
        int[] ids = [.. thirdPartyIds];

        if (ids.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            """
            SELECT
                tpa.IdThirdParty,
                a.Id,
                a.Details,
                a.IdMunicipality,
                a.MunicipalityName,
                a.DepartmentId,
                a.DepartmentName
            FROM dbo.ThirdPartiesAddresses tpa
            INNER JOIN dbo.vw_ThirdPartyAddress a
                ON a.Id = tpa.IdAddress
            WHERE tpa.IdThirdParty IN @ThirdPartyIds;
            """,
            new { ThirdPartyIds = ids },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ThirdPartyAddressSqlDto>(command);

        return rows
        .GroupBy(x => x.IdThirdParty)
        .Select(group => (
            ThirdPartyId: group.Key,
            Addresses: (IEnumerable<Address>)group.Select(ThirdPartyAddressSqlMapper.ToEntity).ToList()
        ));
    }

    // ============================================================
    // Departments
    // ============================================================

    // Entity Framework: guarda los departamentos (usa el Dto, no la entidad)
    public async Task AddDepartmentsAsync(IEnumerable<Department> department, CancellationToken cancellationToken = default)
    {
        DepartmentSqlDto[] dtos = [.. department.Select(DepartmentSqlMapper.ToValue)];

        if (dtos.Length == 0)
            return;

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Departments.AddRange(dtos);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetDepartmentsAsync()
    {
        using var connection = _factory.Create();

        var rows = await connection.QueryAsync<DepartmentSqlDto>(
            "SELECT Id, Name FROM dbo.Departments;");

        return rows.Select(DepartmentSqlMapper.ToEntity).ToList();
    }

    public async Task<IEnumerable<Department>> GetDepartmentsByIdsAsync(IEnumerable<int> id, CancellationToken cancellationToken = default)
    {
        int[] ids = [.. id];

        if (ids.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT Id, Name FROM dbo.Departments WHERE Id IN @Ids;",
            new { Ids = ids },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<DepartmentSqlDto>(command);

        return rows.Select(DepartmentSqlMapper.ToEntity).ToList();
    }

    public async Task<IEnumerable<Department>> GetDepartmentsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        string[] allNames = [.. names];

        if (allNames.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT Id, Name FROM dbo.Departments WHERE Name IN @Names;",
            new { Names = allNames },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<DepartmentSqlDto>(command);

        return rows.Select(DepartmentSqlMapper.ToEntity).ToList();
    }

    // ============================================================
    // Municipalities
    // ============================================================

    public async Task AddMunicipalitiesAsync(IEnumerable<Municipality> municipality, CancellationToken cancellationToken = default)
    {
        MunicipalitySqlDto[] dtos = [.. municipality.Select(MunicipalitySqlMapper.ToValue)];

        if (dtos.Length == 0)
            return;

        await ExecuteInTransactionAsync(
            "InsertMunicipality",
            dtos.Select(x => new { x.Name, IdDept = x.DepartmentId }),
            cancellationToken);
    }

    public async Task<IEnumerable<Municipality>> GetMunicipalitiesAsync()
    {
        using var connection = _factory.Create();

        var rows = await connection.QueryAsync<MunicipalitySqlDto>(
            "SELECT * FROM dbo.vw_MunicipalityDetails;");

        return rows.Select(MunicipalitySqlMapper.ToEntity).ToList();
    }

    public async Task<IEnumerable<Municipality>> GetMunicipalitiesByIdsAsync(IEnumerable<int> id, CancellationToken cancellationToken = default)
    {
        int[] ids = [.. id];

        if (ids.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT * FROM dbo.vw_MunicipalityDetails WHERE Id IN @Ids;",
            new { Ids = ids },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<MunicipalitySqlDto>(command);

        return rows.Select(MunicipalitySqlMapper.ToEntity).ToList();
    }

    public async Task<IEnumerable<Municipality>> GetMunicipalitiesByNameAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        string[] allNames = [.. names];

        if (allNames.Length == 0)
            return [];

        using var connection = _factory.Create();

        var command = new CommandDefinition(
            "SELECT * FROM dbo.vw_MunicipalityDetails WHERE Name IN @Names;",
            new { Names = allNames },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<MunicipalitySqlDto>(command);

        return rows.Select(MunicipalitySqlMapper.ToEntity).ToList();
    }

    // ============================================================
    // Helpers
    // ============================================================

    /// <summary>
    /// Runs a stored procedure once per item inside ONE transaction:
    /// if one insert fails, none of them is saved.
    /// </summary>
    private async Task ExecuteInTransactionAsync<T>(
        string procedureName,
        IEnumerable<T> parameters,
        CancellationToken cancellationToken)
    {
        using SqlConnection connection = _factory.Create();
        await connection.OpenAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            var command = new CommandDefinition(
                procedureName,
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}