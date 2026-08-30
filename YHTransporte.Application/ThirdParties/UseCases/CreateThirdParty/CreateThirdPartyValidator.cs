using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Repositories;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;

public class CreateThirdPartyValidator(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ?? 
    throw new ArgumentNullException(nameof(repository));
    public async Task<OneOf<Success, AlreadyExists,ValidationError>> Validate(CreateThirdPartyCommand[] commands)
    {
        foreach(var command in commands)
            if(string.IsNullOrWhiteSpace(command.Name))
                return new ValidationError(nameof(command));
            
        
        var names = commands
        .Select(x => x.Name)    
        .ToArray();

        var existingNames = await _repository
            .FindExistingNamesAsync(names);

        if (existingNames.Any())
            return new AlreadyExists(existingNames);
        
        return new Success();
    }
}