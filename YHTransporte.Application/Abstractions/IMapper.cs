
namespace YHTransporte.Application.Abstractions;

/// <summary>
/// Can convert an entity into itself and viceversa.
/// Works as a Mapper.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public interface IMapper<TValue, TEntity>
{
    static abstract TValue ToValue(TEntity entity);
    static abstract TEntity ToEntity(TValue value);
}