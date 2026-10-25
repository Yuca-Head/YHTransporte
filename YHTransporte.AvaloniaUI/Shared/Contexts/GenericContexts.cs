using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.Core.Shared;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

internal static class GenericContexts
{

    /// <summary>
    /// May return null, if returns null, you must use model list as result for your OneOf.
    /// That means that queries values already exists in the context.
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
    internal static async Task<TOneOf?> TryGetAsync<TKey, TModel, TQuery, TOneOf>
    (IEnumerable<TQuery> query, 
    Func<TQuery, TKey> queryKeySelector,
    Dictionary<TKey, TModel> modelCenter,
    Func<TModel,TKey> modelKeySelector,
    Func<IEnumerable<TQuery>, CancellationToken, Task<TOneOf>> getHandler,
    IList<TModel> models,
    CancellationToken cancellationToken = default
    ) where TKey : notnull where TOneOf : IOneOf
    {   
        var mQuery = query.ToList();
        TKey[] keys = [..mQuery.Select(queryKeySelector)];
        TKey[] existingIds = [.. modelCenter.Keys.Intersect(keys)];

        foreach(var id in existingIds)
            models.Add(modelCenter[id]);
        
        TQuery[] nonExistingKeys = [.. mQuery.ExceptBy(existingIds, queryKeySelector)];

        TOneOf result;

        if(nonExistingKeys.Length != 0)
        {
            result = await getHandler(nonExistingKeys, cancellationToken);

            if (result.Value is Success<IEnumerable<TModel>> success)
            {
                foreach(var model in success.Value)
                {
                    modelCenter.Add(modelKeySelector(model), model);
                    models.Add(model);
                }
            }
            return result;
        }
        else
            return default;
    }

    internal async static Task UpdateViewModelList<TDto, TVM, TKey, TMapper>
    (
    Dictionary<TKey, TVM> dic, UpdateMessage<TKey> message,
    Func<IEnumerable<TKey>, CancellationToken, Task<IEnumerable<TDto>>> getFunc, 
    Func<TVM, TKey> vmKeySelector,
    CancellationToken  cancellationToken = default)
    where TMapper : IMapper<TVM, TDto> where TKey : notnull
    {


        foreach(TDto item in await getFunc(message.Values, cancellationToken))
        {
            var vm = TMapper.ToValue(item);
            var key = vmKeySelector(vm);
            var inner = dic.GetValueOrDefault(key); 
            

            if(inner is null)
                dic.Add(key, vm);
            else
            {
                dic.Remove(key);
                dic.Add(key, vm);
            }
                
        }
        
    }

    internal async static Task UpdateDtoList<TDto, TKey>
    (
    Dictionary<TKey, TDto> dic, UpdateMessage<TKey> message,
    Func<IEnumerable<TKey>, CancellationToken, Task<IEnumerable<TDto>>> getFunc, 
    Func<TDto, TKey> keySelector,
    CancellationToken  cancellationToken = default) where TKey : notnull
    {
        
        foreach(TDto item in await getFunc(message.Values, cancellationToken))
        {
            var key = keySelector(item);
            var inner = dic.GetValueOrDefault(key);
            

            if(inner is null)
                dic.Add(key, item);
            else
            {
                dic.Remove(key);
                dic.Add(key, item);
            }
                
        }
        
    }
}