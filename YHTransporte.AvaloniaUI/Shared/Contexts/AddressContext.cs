using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.Results;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.Application.Addresses.UseCases.GetAddress;
using YHTransporte.Application.Shared.Results;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

public enum AddressType
{
    Address,
    Department,
    Municipality
}

public sealed class AddressContext : IInitiableContext
{
    
    public AddressContext(GetAddressHandler getHandler)
    {
        _getHandler = getHandler;

        WeakReferenceMessenger.Default.Register<AddressUpdateMessage>(this, async (_,_) =>
        { 
            await UpdateList(_addresses, _getHandler.GetAddressesAsync(), x => x.Id);
            AddressesChanged?.Invoke();
        });
        
        WeakReferenceMessenger.Default.Register<MuinicipalityUpdateMessage>(this, async (_,_) => 
        {
            await UpdateList(_municipalities, _getHandler.GetMunicipalitiesAsync(), x => x.Id);
            MunicipalitiesChanged?.Invoke();
        });

        WeakReferenceMessenger.Default.Register<DepartmentUpdateMessage>(this, async (_,_) => 
        {
            await UpdateList(_departments, _getHandler.GetDepartmentsAsync(), x => x.Id);
            DepartmentsChanged?.Invoke();
        });
    }

    public event Action? DepartmentsChanged;
    public event Action? MunicipalitiesChanged;
    public event Action? AddressesChanged;

    private readonly GetAddressHandler _getHandler;
    private readonly Dictionary<int, DepartmentDto> _departments = [];

    private readonly Dictionary<int, MunicipalityDto> _municipalities = [];

    private readonly Dictionary<int, AddressDetailsDto> _addresses = [];

    public IReadOnlyCollection<AddressDetailsDto> Addresses => _addresses.Values;
    public IReadOnlyCollection<MunicipalityDto> Municipalities => _municipalities.Values;
    public IReadOnlyCollection<DepartmentDto> Departments => _departments.Values;

    public async Task<
    OneOf<Success<IEnumerable<AddressDetailsDto>>, 
    NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>> GetAddressesByKeysAsync
    (IEnumerable<GetAddressQuery> query)
    {
        /*
        esto era por si el generic fallaba 
        int[] keys = [..query.Select(x => x.Id)];
        int[] existingIds = [.. _addresses.Keys.Intersect(keys)];

        List<AddressDetailsDto> addresses = [];

        foreach(var id in existingIds)
            addresses.Add(_addresses[id]);
        
        GetAddressQuery[] nonExistingIds = [.. query.ExceptBy(existingIds, x => x.Id)];

        var result = await _getHandler.GetAddressesByIdsAsync(nonExistingIds);

        if (result.IsT0)
        {
            foreach(var address in result.AsT0.Value)
            {
                addresses.Add(address);
                _addresses.Add(address.Id, address);
            }
            return new Success<IEnumerable<AddressDetailsDto>>(addresses);
        }
        else
            return result;
            */

        
        List<AddressDetailsDto> addresses = [];
        var result = await GenericContexts.TryGetAsync
        (query, x => x.Id, _addresses, x => x.Id,
        _getHandler.GetAddressesByIdsAsync, addresses);
        
        if(result.IsT0 && result.AsT0.Value is null)
            return new Success<IEnumerable<AddressDetailsDto>>(addresses);

        return result;
    }

    public async Task<OneOf<Success<IEnumerable<MunicipalityDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>> GetMunicipalitiesByKeysAsync
    (IEnumerable<GetAddressQuery> query)
    {
        List<MunicipalityDto> municipalities = [];
        var result = await GenericContexts.TryGetAsync
        (query, x => x.Id, _municipalities, x => x.Id,
        _getHandler.GetMunicipalitiesByIdsAsync, municipalities);
        
        if(result.IsT0 && result.AsT0.Value is null)
            return new Success<IEnumerable<MunicipalityDto>>(municipalities);

        return result;
    }
    


    public async Task<OneOf<Success<IEnumerable<DepartmentDto>>, NotFound<IEnumerable<int>>, 
    RepeatedValue<IEnumerable<RepeatedValue<int>.RepeatedKeyInformation>>>>
    GetDepartmentsByIdsAsync(IEnumerable<GetAddressQuery> query)
    {
        List<DepartmentDto> departments = [];
        
        var result = await GenericContexts.TryGetAsync
        (query, x => x.Id, _departments, x => x.Id,
        _getHandler.GetDepartmentsByIdsAsync, departments);
        
        if(result.IsT0 && result.AsT0.Value is null)
            return new Success<IEnumerable<DepartmentDto>>(departments);

        return result;
    }

    public async Task Init()
    {
        
        foreach(var v in await _getHandler.GetAddressesAsync())
            _addresses.Add(v.Id, v);
        foreach(var v in await _getHandler.GetMunicipalitiesAsync())
            _municipalities.Add(v.Id, v);
        foreach(var v in await _getHandler.GetDepartmentsAsync())
            _departments.Add(v.Id, v);
    }

    private static async Task UpdateList<T>(Dictionary<int, T> dict, Task<IEnumerable<T>> collection, Func<T, int> selector)
    {
        dict.Clear();

        foreach(var value in await collection)
            dict.Add(selector(value), value);
    }

}