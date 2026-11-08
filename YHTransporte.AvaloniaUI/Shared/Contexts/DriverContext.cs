using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Deliveries.UseCases.GetDriver;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

/// <summary>
/// Caché en memoria de los conductores, indexados por su código (DRV-0000).
/// </summary>
public sealed class DriverContext : IInitiableContext
{
    private readonly GetDriverHandler _queryHandler;
    private readonly Dictionary<string, DriverDto> _drivers = new(StringComparer.OrdinalIgnoreCase);

    public event Action? DriversChanged;

    public IReadOnlyCollection<DriverDto> Drivers => [.. _drivers.Values.OrderBy(x => x.Code)];

    /// <summary>Conductores que no están en un envío.</summary>
    public IReadOnlyCollection<DriverDto> AvailableDrivers => [.. Drivers.Where(x => !x.InUse)];

    public DriverContext(GetDriverHandler queryHandler)
    {
        _queryHandler = queryHandler;

        WeakReferenceMessenger.Default.Register<DriverUpdateMessage>(this, async (_, m) =>
        {
            if(m.Values is null || m.ChangeType is Enums.ContextChangeType.Creation)
                await ReloadAsync();
            else
                await GenericContexts.UpdateDtoList(_drivers, m,
                async (codes, token) =>
                {
                    List<DriverDto> drivers = [];

                    foreach(var code in codes)
                    {
                        var result = await _queryHandler.GetDriverAsync(code, token);

                        if(!result.IsT0)
                            throw new InvalidOperationException("Every Driver code was expected to exist");

                        drivers.Add(result.AsT0.Value);
                    }

                    return drivers;
                }, d => d.Code);

            DriversChanged?.Invoke();
        });
    }

    public async Task Init() => await ReloadAsync();

    public DriverDto? GetByCode(string code) => _drivers.GetValueOrDefault(code.Trim());

    private async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _drivers.Clear();

        foreach(var driver in await _queryHandler.GetDriversAsync(cancellationToken))
            _drivers.Add(driver.Code.Trim(), driver);
    }
}
