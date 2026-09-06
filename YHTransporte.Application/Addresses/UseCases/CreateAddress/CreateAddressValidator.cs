using OneOf;
using OneOf.Types;
using YHTransporte.Application.Addresses.Comparers;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Addresses.UseCases.CreateAddress;

internal static class CreateAddressValidator
{
    internal static CreateAddressComparer Comparer {get;} = new();
    /// <summary>
    /// Generic handler to create an address
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    /// <param name="commands"></param>
    /// <param name="function">GetbyName method from repository</param>
    /// <param name="placesFunction">Function to get the address location, must get just locations key</param>
    /// <remarks>
    /// If result is AlreadyExists, should check if already existing PlaceId <c>T</c>(municipality id / department id) are
    /// the same of the commands before accept or cancel.
    /// </remarks>
    internal static async Task<OneOf<
    Success, 
    ValidationError, 
    AlreadyExists<IEnumerable<T>>, 
    RepeatedValue<IEnumerable <RepeatedValue<AddressKey>.RepeatedKeyInformation> >,
    NotFound<IEnumerable<int>>
    >>
    Validate<T>(IEnumerable<AddressKey> commands, 
    Func<IEnumerable<string>, CancellationToken, Task<IEnumerable<T>>> getValuesByName,
    Func<IEnumerable<int>, Task<IEnumerable<int>>>? getExistingPlaces)
    {
        var commandList = commands.ToList();

        //Validates commands.
        AddressKey? invalidCommand = commandList.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Name));

        if(invalidCommand  is not null)
            return new ValidationError(nameof(invalidCommand.Name), "Debe ingresar un nombre para crear una dirección");//T1

        //If a command is repeated returns the repeated values.
        var repeatedResult = MinimalValidator.ValidateForRepeatedKeys(commandList, Comparer);

        if(repeatedResult.IsT1)
            return repeatedResult.AsT1; //It is RepeatedValue (T3)

        //here Checks if the places where addresses belong actually exist
        if(getExistingPlaces is not null)
        {
            var owners = commandList.Select(x => x.PlaceId).Distinct();
            var distincts = owners.Except(await getExistingPlaces(owners)).ToArray();

            if(distincts.Length != 0)
                return new NotFound<IEnumerable<int>>(distincts); //T4
        }
        
        
        var addresses = (await getValuesByName(commandList.Select(x => x.Name), default)).ToList();
        
        if(addresses.Count != 0)
            return new AlreadyExists<IEnumerable<T>>(addresses); //T2
    
        return new Success(); //T0
    }
}