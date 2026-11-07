using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.UseCases.GetShipment;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

public sealed class ShipmentContext : IInitiableContext
{
    private readonly GetShipmentHandler _queryHandler;
    private readonly Dictionary<string, ShipmentDto> _shipments = [];  // el «cuaderno» en memoria

    public event Action? ShipmentsChanged;                            // indica a las pantallas que se actualicen

    public IReadOnlyCollection<ShipmentDto> Shipments => [.. _shipments.Values];

    public ShipmentContext(GetShipmentHandler queryHandler)
    {
        _queryHandler = queryHandler;

        WeakReferenceMessenger.Default.Register<ShipmentUpdateMessage>(this, async (_, m) =>
        {
            if (m.Values is null)
                await ReloadAsync();    // nadie indicó cuáles cambiaron: recargar todo
            else
                await GenericContexts.UpdateDtoList(_shipments, m,
                    async (keys, token) =>
                    {
                        var result = await _queryHandler.GetShipmentsByCodesAsync(
                            keys.Select(k => new GetShipmentQuery(k)), token);

                        if (!result.IsT0)
                            throw new InvalidOperationException("Every Shipment key was expected to exist");
                        return result.AsT0.Value;
                    },
                    d => d.Key);

            ShipmentsChanged?.Invoke();
        });
    }

    public async Task Init() => await ReloadAsync();

    private async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _shipments.Clear();

        foreach (var shipment in await _queryHandler.GetShipments(cancellationToken))
            _shipments.Add(shipment.Key, shipment);
    }

    public ShipmentDto? GetById(string key) => _shipments.GetValueOrDefault(key);

    public IReadOnlyCollection<ShipmentDto> GetByOrder(string orderKey)
    => [.. _shipments.Values.Where(x => x.OrderKey == orderKey)];

    /// <summary>
    /// Lo que ya se encuentra en la caché proviene de la memoria; solo las claves faltantes se buscan en la base de datos.
    /// </summary>
    public async Task<OneOf<Success<IEnumerable<ShipmentDto>>, NotFound<IEnumerable<string>>,
    RepeatedValue<IEnumerable<RepeatedValue<string>.RepeatedKeyInformation>>>>
    GetShipmentsByKeysAsync(IEnumerable<GetShipmentQuery> query, CancellationToken cancellationToken = default)
    {
        List<ShipmentDto> shipments = [];

        var result = await GenericContexts.TryGetAsync
        (query, x => x.Key, _shipments, x => x.Key,
        _queryHandler.GetShipmentsByCodesAsync, shipments, cancellationToken);

        // IsT0 = todo fue encontrado (caché, base de datos o una combinación) y «shipments» contiene la totalidad de los datos.
        return result.IsT0 ? new Success<IEnumerable<ShipmentDto>>(shipments) : result;
    }
}
