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

    //Este resumen se lo pedí a la IA porque no me acordaba que hacía esto,
    //que conste que si lo hice yo xd.
    /// <summary>
    /// Retrieves the requested models using an in-memory cache.
    /// Models already present in <paramref name="modelCenter"/> are added
    /// to <paramref name="models"/> without invoking the handler.
    /// Only missing models are requested through <paramref name="getHandler"/>.
    /// Successfully retrieved models are added to both collections.
    /// </summary>
    /// <typeparam name="TKey">The type of the model identifier.</typeparam>
    /// <typeparam name="TModel">The type of the models being retrieved.</typeparam>
    /// <typeparam name="TQuery">The type of the query elements.</typeparam>
    /// <typeparam name="TOneOf">The result type returned by the handler.</typeparam>
    /// <param name="query">The elements whose corresponding models are requested.</param>
    /// <param name="queryKeySelector">Extracts the identifier from a query element.</param>
    /// <param name="modelCenter">The in-memory cache indexed by model identifiers.</param>
    /// <param name="modelKeySelector">Extracts the identifier from a retrieved model.</param>
    /// <param name="getHandler">Retrieves the models corresponding to the missing queries.</param>
    /// <param name="models">The collection where cached and successfully retrieved models are accumulated.</param>
    /// <param name="cancellationToken">A token used to cancel the retrieval operation.</param>
    /// <returns>
    /// The handler result if any models were missing; otherwise, <see langword="null"/>
    /// when all requested models were already present in the cache.
    /// </returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentNullException">If message has no values</exception>
    /// <returns></returns>
    internal async static Task UpdateDtoListByOne<TDto, TKey>
    (
    Dictionary<TKey, TDto> dic, UpdateMessage<TKey> message,
    Func<TKey, CancellationToken, Task<TDto>> getFunc, 
    Func<TDto, TKey> keySelector,
    CancellationToken  cancellationToken = default) where TKey : notnull
    {

        var first = message.Values!.First(); //May throw exception
        
        var item = await getFunc(first, cancellationToken);
        
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