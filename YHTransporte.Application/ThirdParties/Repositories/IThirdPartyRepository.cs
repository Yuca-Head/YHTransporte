using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.Repositories;

public interface IThirdPartyRepository : IRepository<int, ThirdParty>
{
    Task<bool> NameExists(string name);
    Task<IEnumerable<string>>FindExistingNamesAsync(params IEnumerable<string> names);
}