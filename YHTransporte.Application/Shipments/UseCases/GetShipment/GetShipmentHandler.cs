using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Entities;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.Mappers;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shared;

namespace YHTransporte.Application.Shipments.UseCases.GetShipment;

public sealed class GetShipmentHandler(IShipmentRepository repository)
{
    private readonly IShipmentRepository _repository = repository ?? throw new ArgumentNullException();

    public async Task<IEnumerable<ShipmentDto>> GetShipments(CancellationToken cancellationToken = default)
    => (await _repository.GetEverythingAsync(cancellationToken)).Select(ShipmentMapper.ToValue);

    public async Task<OneOf<Success<ShipmentDto>, OneOf.Types.NotFound>> GetShipmentByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var shipment = (await _repository.GetByKeysAsync([code], cancellationToken)).FirstOrDefault();

        
        return shipment is not null ? new Success<ShipmentDto>(ShipmentMapper.ToValue(shipment)) :
        new OneOf.Types.NotFound();
    }

    public async Task<OneOf<Success<IEnumerable<ShipmentDto>>, NotFound<IEnumerable<string>>,
    RepeatedValue<IEnumerable<RepeatedValue<string>.RepeatedKeyInformation>>>>
    GetShipmentsByCodesAsync(IEnumerable<GetShipmentQuery> query, CancellationToken cancellationToken = default)
    => await GenericGetHandler.HandleByKey
    (query, x => x.Key, _repository.GetByKeysAsync, ShipmentMapper.ToValue, x => x.Key, cancellationToken);

}