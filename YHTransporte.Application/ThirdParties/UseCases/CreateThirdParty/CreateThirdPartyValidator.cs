using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Repositories;

namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;

public class CreateThirdPartyValidator(IThirdPartyRepository repository)
{
    private readonly IThirdPartyRepository _repository = repository ?? 
    throw new ArgumentNullException(nameof(repository));
    


    public async Task<OneOf<Success, 
    AlreadyExists<IEnumerable<string>>, ValidationError, RepeatedValue<IEnumerable<string>>>> Validate(CreateThirdPartyCommand[] commands)
    {
        HashSet<string> names = new(StringComparer.CurrentCultureIgnoreCase);
        HashSet<string> repeatedNames = [];

        foreach(var command in commands)
            if(string.IsNullOrWhiteSpace(command.Name))
                return new ValidationError(nameof(command.Name), ["Debe ingresar un nombre para crear un tercero"]);    
            else if(!names.Add(command.Name))
                repeatedNames.Add(command.Name);

        if(repeatedNames.Count != 0)
            return new RepeatedValue<IEnumerable<string>>(repeatedNames);

        var existingNames = await _repository
            .FindExistingNamesAsync(names);
        
        if (existingNames.Any())
            return new AlreadyExists<IEnumerable<string>>(existingNames);
        
        return new Success();
    }
}