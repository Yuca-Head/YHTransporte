using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Mappers;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;

public sealed class CreateThirdPartyHandler(IThirdPartyRepository repository, CreateThirdPartyValidator validator)
{
    private readonly IThirdPartyRepository _repository = repository ??
    throw new ArgumentNullException(nameof(repository));

    private readonly CreateThirdPartyValidator _validator = validator ??
    throw new ArgumentNullException(nameof(validator));

    public async Task<OneOf<Success, AlreadyExists, ValidationError, RepeatedValue>> Handle(params CreateThirdPartyCommand[] commands)
    {
        List<ThirdParty> thirdParties = [];

        var result = await _validator.Validate(commands);   

        if(result.IsT1)
            return result.AsT1;
        if(result.IsT2) 
            return result.AsT2;
        if(result.IsT3)
            return result.AsT3;

        
        foreach (var command in commands)
        {
            var thirdParty = new ThirdParty(command.Name);

            if (command.IsCustomer)
                thirdParty.BecomeCustomer();

            if (command.IsSupplier)
                thirdParty.BecomeSupplier();

            thirdParties.Add(thirdParty);
        }

        await _repository.AddAsync(thirdParties);


        return new Success();
    }
}