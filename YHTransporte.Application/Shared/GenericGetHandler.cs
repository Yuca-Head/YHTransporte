using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Shared;

internal static class GenericGetHandler
{
    internal static async Task<OneOf<
    Success<IEnumerable<TDto>>,
    NotFound<IEnumerable<int>>,
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>> 
    HandleById<TEntity, TDto, TQuery>
    (IEnumerable<TQuery> query,
    Func<TQuery, int> querySelector,
    Func<IEnumerable<int>, CancellationToken, Task<IEnumerable<TEntity>>> getFunction, 
    Func<TEntity, TDto> converter,
    Func<TDto, int> dtoSelector,
    CancellationToken cancellationToken = default)
    {
        int[] ids = [..query.Select(querySelector)];
        var commandsValidator = MinimalValidator.ValidateForRepeatedKeys(ids);

        if(commandsValidator.IsT1)
            return commandsValidator.AsT1;

        var values = (await getFunction(ids, cancellationToken))
            .Select(converter)
            .ToList();
        
        return values.Count == ids.Length ?
            new Success<IEnumerable<TDto>> (values) :
            new NotFound<IEnumerable<int>> (ids.Except(values.Select(dtoSelector)));
    }

    
    /// <summary>
    /// Igual que HandleById, pero funciona con cualquier tipo de clave (por ejemplo, string).
    /// </summary>
    // Aún así le falta usar comparers, pero bueno
    internal static async Task<OneOf<
    Success<IEnumerable<TDto>>,
    NotFound<IEnumerable<TKey>>,
    RepeatedValue<IEnumerable<RepeatedValue<TKey>.RepeatedKeyInformation>>>>
    HandleByKey<TEntity, TDto, TQuery, TKey>
    (IEnumerable<TQuery> query,
    Func<TQuery, TKey> querySelector,
    Func<IEnumerable<TKey>, CancellationToken, Task<IEnumerable<TEntity>>> getFunction,
    Func<TEntity, TDto> converter,
    Func<TDto, TKey> dtoSelector,
    CancellationToken cancellationToken = default)
    {
        TKey[] keys = [.. query.Select(querySelector)];
        var keysValidator = MinimalValidator.ValidateForRepeatedKeys(keys);

        if (keysValidator.IsT1)
            return keysValidator.AsT1;

        var values = (await getFunction(keys, cancellationToken))
            .Select(converter)
            .ToList();

        return values.Count == keys.Length ?
            new Success<IEnumerable<TDto>>(values) :
            new NotFound<IEnumerable<TKey>>(keys.Except(values.Select(dtoSelector)));
    }
}