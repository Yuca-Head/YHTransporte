using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Deliveries.UseCases.GetVehicle;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

/// <summary>
/// Caché en memoria de los vehículos (igual que ThirdPartyContext / OrderContext).
/// </summary>
public sealed class VehicleContext : IInitiableContext
{
    private readonly GetVehicleHandler _queryHandler;
    private readonly Dictionary<int, VehicleDto> _vehicles = [];

    public event Action? VehiclesChanged;

    public IReadOnlyCollection<VehicleDto> Vehicles => [.. _vehicles.Values.OrderBy(x => x.Id)];

    /// <summary>Vehículos que no están en un envío.</summary>
    public IReadOnlyCollection<VehicleDto> AvailableVehicles => [.. Vehicles.Where(x => !x.InUse)];

    public VehicleContext(GetVehicleHandler queryHandler)
    {
        _queryHandler = queryHandler;

        WeakReferenceMessenger.Default.Register<VehicleUpdateMessage>(this, async (_, m) =>
        {
            if(m.Values is null || m.ChangeType is Enums.ContextChangeType.Creation)
                await ReloadAsync();
            else
                await GenericContexts.UpdateDtoList(_vehicles, m,
                async (keys, token) =>
                {
                    List<VehicleDto> vehicles = [];

                    foreach(var key in keys)
                    {
                        var result = await _queryHandler.GetVehicleAsync(key, token);

                        if(!result.IsT0)
                            throw new InvalidOperationException("Every Vehicle key was expected to exist");

                        vehicles.Add(result.AsT0.Value);
                    }

                    return vehicles;
                }, v => v.Id);

            VehiclesChanged?.Invoke();
        });
    }

    public async Task Init() => await ReloadAsync();

    public VehicleDto? GetById(int id) => _vehicles.GetValueOrDefault(id);

    private async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _vehicles.Clear();

        foreach(var vehicle in await _queryHandler.GetVehiclesAsync(cancellationToken))
            _vehicles.Add(vehicle.Id, vehicle);
    }
}
