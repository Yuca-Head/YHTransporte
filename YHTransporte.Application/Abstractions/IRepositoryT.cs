using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Shared;

namespace YHTransporte.Application.Abstractions;

public interface IRepository<TKey, TValue> : IRepository where TValue : IEntity<TKey>
{
    Task AddAsync(TValue entity, CancellationToken cancellationToken = default);
    Task AddAsync(IEnumerable<TValue> entities, CancellationToken cancellationToken = default);
    Task<IEnumerable<TValue>> GetByKeysAsync(IEnumerable<TKey> key, CancellationToken cancellationToken = default);
    Task<IEnumerable<TValue>> GetEverythingAsync(CancellationToken cancellationToken = default);
    Task<bool> Exists(TKey key, CancellationToken cancellationToken = default);
}