using System.Data;
using Dapper;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

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
    {
        foreach(var e in entities)
            await AddAsync(e, cancellationToken);
    }
    
    public Task<bool> Exists(int key)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> FindExistingNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        return [];
    }

    public Task<ThirdParty?> GetByKeyAsync(int key, CancellationToken cancellationToken = default)
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