
namespace YHTransporte.Application.Abstractions;

/// <summary>
/// Can convert an entity into itself and viceversa.
/// Works as a Mapper.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public interface IMapper<TValue, TEntity>
{
    TValue ToValue(TEntity enity);
    TEntity ToEntity(TValue value);
}