using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Repositories;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;

public class CreateThirdPartyValidator(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ?? 
    throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<Success, AlreadyExists, ValidationError, RepeatedValue>> Validate(CreateThirdPartyCommand[] commands)
    {
        HashSet<string> names = new(StringComparer.CurrentCultureIgnoreCase);

        foreach(var command in commands)
            if(string.IsNullOrWhiteSpace(command.Name))
                return new ValidationError(nameof(command));
            else if(!names.Add(command.Name))
                return new RepeatedValue(command.Name);
        

        var existingNames = await _repository
            .FindExistingNamesAsync(names);
        
        if (existingNames.Any())
            return new AlreadyExists(existingNames);
        
        return new Success();
    }
}