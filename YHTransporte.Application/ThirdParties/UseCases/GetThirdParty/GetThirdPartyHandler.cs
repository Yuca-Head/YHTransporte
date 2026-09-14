using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Mappers;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;

public sealed class GetThirdPartyHandler(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ??
    throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<Success<IEnumerable<ThirdPartyDetailsDto>>, IEnumerable<ThirdPartyNotFound>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetThirdPartyDetails(IEnumerable<GetThirdPartyQuery> query)
    => await Handle<ThirdPartyDetailsDto>(query, x => new(x.Name, x.Addresses, x.Key, x.Customer, x.Supplier), x => x.Key);

    

    private async Task<OneOf<Success<IEnumerable<T>>, IEnumerable<ThirdPartyNotFound>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>> 
    Handle<T>(IEnumerable<GetThirdPartyQuery> query, Func<ThirdParty, T> converter, Func<T, int> dtoSelector)
    {
        var result = await GenericGetHandler.HandleById
        (query, x => x.Key, _repository.GetByKeysAsync, converter, dtoSelector);

        if(result.IsT0)
            return result.AsT0;

        if(result.IsT1)
            return result.AsT1.Argument.Select(x => new ThirdPartyNotFound(x)).ToArray();
        
        return result.AsT2;
    }
}