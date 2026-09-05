using OneOf;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.Mappers;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Shared.Results;
using System.Linq;

namespace YHTransporte.Application.Addresses.UseCases.GetAddress;

public sealed class GetAddressHandler(IAddressRepository repository)
{
    private readonly IAddressRepository _repository = repository ??
    throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<IEnumerable<AddressDetailsDto>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetAddressesAsync(IEnumerable<GetAddressCommand> commands)
    {
        
        var commandsValidator = MinimalValidator.ValidateForRepeatedKeys(commands.Select(x => x.Id));

        if(commandsValidator.IsT1)
            return commandsValidator.AsT1;
    
        var values = (await _repository.GetManyByKeysAsync(
            commands.Select(x => x.Id)))
            .Select(AddressMapper.AddressToDetailedDto)
            .ToList();

        return values.Count == commands.Count() ?
            values : 
            new NotFound<IEnumerable<int>>(commands.Select(x => x.Id).Except(values.Select(x => x.Id)));
    }
}