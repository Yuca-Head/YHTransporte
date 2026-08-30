using OneOf;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Mappers;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;

namespace YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;

public sealed class GetThirdPartyHandler(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ??
    throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<ThirdPartyDto, ThirdPartyDetailsDto, ThirdPartyNotFound>> Handle(GetThirdPartyQuery query)
    {
        var entity = await _repository.GetByKeyAsync(query.Key);

        if(entity is null)
            return new ThirdPartyNotFound(query.Key);

        return query.GetDetailed
        ? ThirdPartyMappers.ToDetailedDto(entity)
        : ThirdPartyMappers.ToDto(entity);
    }
}