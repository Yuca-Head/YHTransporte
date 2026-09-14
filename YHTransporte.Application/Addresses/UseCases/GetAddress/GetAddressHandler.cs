using OneOf;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.Mappers;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Shared.Results;
using System.Linq;
using YHTransporte.Core.Shared;
using YHTransporte.Core.Entities;
using OneOf.Types;

namespace YHTransporte.Application.Addresses.UseCases.GetAddress;

public sealed class GetAddressHandler(IAddressRepository repository)
{
    private readonly IAddressRepository _repository = repository ??
    throw new ArgumentNullException(nameof(repository));

    public async Task<OneOf<
    Success<IEnumerable<AddressDetailsDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetAddressesByIdsAsync(IEnumerable<GetAddressQuery> query, CancellationToken cancellationToken = default)
    =>  await GenericGetHandler.HandleById
    (query, x => x.Id, _repository.GetByKeysAsync, AddressMappers.AddressToDetailedDto, x => x.Id, cancellationToken);
    
    public async Task<OneOf<Success<IEnumerable<MunicipalityDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetMunicipalitiesByIdsAsync(IEnumerable<GetAddressQuery> query)
    => await GenericGetHandler.HandleById
    (query, x => x.Id, _repository.GetMunicipalitiesByIdsAsync, AddressMappers.MunicipalityToDto, x => x.Id);

    public async Task<OneOf<Success<IEnumerable<DepartmentDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetDepartmentsByIdsAsync(IEnumerable<GetAddressQuery> query)
    => await GenericGetHandler.HandleById
    (query, x => x.Id, _repository.GetDepartmentsByIdsAsync, AddressMappers.DepartmentToDto, x => x.Id);

    public async Task<IEnumerable<AddressDetailsDto>> GetAddressesAsync()
    => (await _repository.GetEverythingAsync()).Select(AddressMappers.AddressToDetailedDto);

    public async Task<IEnumerable<MunicipalityDto>> GetMunicipalitiesAsync()
    => (await _repository.GetMunicipalitiesAsync()).Select(AddressMappers.MunicipalityToDto);
    
    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync()
    => (await _repository.GetDepartmentsAsync()).Select(AddressMappers.DepartmentToDto);
}