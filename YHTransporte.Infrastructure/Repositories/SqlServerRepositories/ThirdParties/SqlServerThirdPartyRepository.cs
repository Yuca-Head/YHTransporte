using System.Data;
using Dapper;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Mappers;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties;

public sealed class SqlServerThirdPartyRepository(DbConnectionFactory factory) : IThirdPartyRepository
{
    private readonly DbConnectionFactory _factory = factory;
    public async Task AddAsync(
        ThirdParty entity,
        CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        ThirdPartySqlDto dto = new(entity.Key, entity.Name, entity.Supplier != null, entity.Customer != null);

        var command = new CommandDefinition(
            "InsertThirdParty",
        new
        {
            dto.Name,
            dto.IsSupplier,
            dto.IsCustomer
        },
        commandType: CommandType.StoredProcedure,
        cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
    public async Task AddAsync(IEnumerable<ThirdParty> entities, CancellationToken cancellationToken = default)
    => await Task.WhenAll(entities.Select(async x =>
    {
        await AddAsync(x, cancellationToken);
    }));
    
    
    public Task<bool> Exists(int key)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Exists(int key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> FindExistingNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        return [];
    }

    public async Task<IEnumerable<ThirdParty>> GetByKeysAsync(IEnumerable<int> keys, CancellationToken cancellationToken = default)
    {
        using var connection = _factory.Create();

        int[] cuteKeys = [.. keys];

        if(/*there´s no*/ cuteKeys.Length == 0)
            return [];

        var command = new CommandDefinition(
            """
            SELECT
                Id,
                Name,
                IsSupplier,
                IsCustomer
            FROM ThirdParties
            WHERE Id IN @Keys;
            """,
            new { Keys = cuteKeys },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ThirdPartySqlDto>(command);

        return rows.Select(ThirdPartySqlMapper.ToEntity);
    }

    public async Task<IEnumerable<ThirdParty>> GetEverythingAsync(CancellationToken cancellationToken = default)
    {
        var connection = _factory.Create();

        var thirdParties = await connection.QueryAsync<ThirdPartySqlDto>
        (
            "Select * from ThirdParties",
            cancellationToken
        );

        return thirdParties.Select(ThirdPartySqlMapper.ToEntity);
    }

    public Task<IEnumerable<ThirdParty>> GetManyByKeysAsync(IEnumerable<int> keys, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> NameExists(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ThirdParty>?> TakeManyAsync(int take, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}