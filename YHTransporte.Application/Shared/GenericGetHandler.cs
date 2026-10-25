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

}