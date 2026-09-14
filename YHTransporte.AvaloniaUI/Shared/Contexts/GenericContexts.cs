using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

public static class GenericContexts
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TOneOf"></typeparam>
    /// <param name="query"></param>
    /// <param name="queryKeySelector"></param>
    /// <param name="modelCenter"></param>
    /// <param name="modelKeySelector"></param>
    /// <param name="result"></param>
    /// <param name="getHandler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal static async Task<object> TryGetAsync<TKey, TModel, TQuery>
    (IEnumerable<TQuery> query, 
    Func<TQuery, TKey> queryKeySelector,
    Dictionary<TKey, TModel> modelCenter,
    Func<TModel,TKey> modelKeySelector,
    Func<IEnumerable<TQuery>, CancellationToken, Task<object>> getHandler,
    IList<TModel> models,
    CancellationToken cancellationToken = default
    ) where TKey : notnull
    {   
        var mQuery = query.ToList();
        TKey[] keys = [..mQuery.Select(queryKeySelector)];
        TKey[] existingIds = [.. modelCenter.Keys.Intersect(keys)];

        foreach(var id in existingIds)
            models.Add(modelCenter[id]);
        
        TQuery[] nonExistingKeys = [.. mQuery.ExceptBy(existingIds, queryKeySelector)];

        object result = await getHandler(nonExistingKeys, cancellationToken);

        if (result is IOneOf oneOf && oneOf.Value is Success<IEnumerable<TModel>> success)
        {
            foreach(var model in success.Value)
            {
                modelCenter.Add(modelKeySelector(model), model);
                models.Add(model);
            }
        }

        return result;
    }
}