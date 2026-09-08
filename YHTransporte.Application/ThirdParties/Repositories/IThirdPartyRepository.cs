using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Repositories;

public interface IThirdPartyRepository : IRepository<int, ThirdParty>
{
    Task<bool> NameExists(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>>FindExistingNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}