using OneOf;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Shared.Results;
using System.Linq;
using YHTransporte.Core.Shared;
using YHTransporte.Core.Entities;
using OneOf.Types;
using YHTransporte.Application.Addresses.AddressMappers;

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
    (query, x => x.Id, _repository.GetByKeysAsync, AddressDetailsMapper.ToValue, x => x.Id, cancellationToken);
    
    public async Task<OneOf<Success<IEnumerable<MunicipalityDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetMunicipalitiesByIdsAsync(IEnumerable<GetAddressQuery> query, CancellationToken cancellationToken)
    => await GenericGetHandler.HandleById
    (query, x => x.Id, _repository.GetMunicipalitiesByIdsAsync, MunicipalityMapper.ToValue, x => x.Id, cancellationToken);

    public async Task<OneOf<Success<IEnumerable<DepartmentDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetDepartmentsByIdsAsync(IEnumerable<GetAddressQuery> query, CancellationToken cancellationToken)
    => await GenericGetHandler.HandleById
    (query, x => x.Id, _repository.GetDepartmentsByIdsAsync, DepartmentMapper.ToValue, x => x.Id, cancellationToken);

    public async Task<IEnumerable<AddressDetailsDto>> GetAddressesAsync()
    => (await _repository.GetEverythingAsync()).Select(AddressDetailsMapper.ToValue);

    public async Task<IEnumerable<MunicipalityDto>> GetMunicipalitiesAsync()
    => (await _repository.GetMunicipalitiesAsync()).Select(MunicipalityMapper.ToValue);
    
    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync()
    => (await _repository.GetDepartmentsAsync()).Select(DepartmentMapper.ToValue);
}