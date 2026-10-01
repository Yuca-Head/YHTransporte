using YHTransporte.Application.Addresses.Repositories;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Addresses.Comparers;
using YHTransporte.Core.Entities;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.Results;
using YHTransporte.Application.Addresses.AddressMappers;

namespace YHTransporte.Application.Addresses.UseCases.CreateAddress;

public sealed class CreateAddressHandler(IAddressRepository repository)
{
    private readonly IAddressRepository _repository = repository ?? 
    throw new ArgumentNullException(nameof(repository));




    public async Task<OneOf<
    Success, 
    ValidationError, 
    AlreadyExists<IEnumerable<(string Details, string Municipality)>>, 
    RepeatedValue<IEnumerable<RepeatedValue<AddressKey>.RepeatedKeyInformation>>,
    MunicipalityNotFound>>
    HandleAddress(IEnumerable<AddressKey> commands)
    {
        var result = await CreateAddressValidator.Validate
        (commands, _repository.GetAddressesByNamesAsync, 
        async x => (await _repository.GetMunicipalitiesByIdsAsync(x)).Select(x => x.Key));


        //Repeated Value
        if(result.IsT3) 
            return result.AsT3;
        
        //Validation Error
        if(result.IsT1)
            return result.AsT1;

        //Municipality not found
        if(result.IsT4)
            return new MunicipalityNotFound(result.AsT4.Argument);
    
        //Already Exists
        if(result.IsT2)
        {
            var repeated = result.AsT2.Argument.IntersectBy
            (commands,
            x => new AddressKey(x.Details, x.Municipality.Key), CreateAddressValidator.Comparer).ToArray();
            
            if(repeated.Length != 0)
                return new AlreadyExists<IEnumerable<(string, string)>> 
                (repeated.Select(x => (x.Details, Municipality : x.Municipality.Name)));
        }


        //Success
        await _repository.AddAsync(commands.Select(AddressKeyMappers.AddressMapper.ToEntity));

        return new Success();
    }   

    public async Task<OneOf<
    Success, 
    ValidationError, 
    AlreadyExists<IEnumerable<(string Details, string Department)>>, 
    RepeatedValue<IEnumerable<RepeatedValue<AddressKey>.RepeatedKeyInformation>>,
    DepartmentNotFound>>

    HandleMunicipality(IEnumerable<AddressKey> commands)
    {
        var result = await CreateAddressValidator.Validate
        (commands, _repository.GetMunicipalitiesByNameAsync, 
        async x => (await _repository.GetDepartmentsByIdsAsync(x)).Select(x => x.Key));


        //Repeated Value
        if(result.IsT3) 
            return result.AsT3;
        
        //Validation Error
        if(result.IsT1)
            return result.AsT1;

        //Department not found
        if(result.IsT4)
            return new DepartmentNotFound(result.AsT4.Argument);
    
        //Already Exists
        if(result.IsT2)
        {
            var repeated = result.AsT2.Argument.IntersectBy
            (commands,
            x => new AddressKey(x.Name, x.Department.Key), CreateAddressValidator.Comparer).ToArray();
            
            if(repeated.Length != 0)
                return new AlreadyExists<IEnumerable<(string, string)>> 
                (repeated.Select(x => (x.Name, Department : x.Department.Name)));
        }


        //Success
        await _repository.AddMunicipalitiesAsync(commands.Select(AddressKeyMappers.MunicipalityMapper.ToEntity));

        return new Success();
    }   


    public async Task<OneOf<
    Success, 
    ValidationError, 
    AlreadyExists<IEnumerable<string>>, 
    RepeatedValue<IEnumerable<RepeatedValue<AddressKey>.RepeatedKeyInformation>>>>
    HandleDepartment(IEnumerable<AddressKey> commands)
    {
        var result = await CreateAddressValidator.Validate
        (commands, _repository.GetDepartmentsByNamesAsync, null);

        //Repeated Value
        if(result.IsT3) 
            return result.AsT3;
        
        //Validation Error
        if(result.IsT1)
            return result.AsT1;
    
        //Already Exists
        if(result.IsT2)
            return new AlreadyExists<IEnumerable<string>>(result.AsT2.Argument.Select(x => x.Name));

        //Success
        await _repository.AddDepartmentsAsync(commands.Select(x => new Department(x.Name)));

        return new Success();
    }   
}