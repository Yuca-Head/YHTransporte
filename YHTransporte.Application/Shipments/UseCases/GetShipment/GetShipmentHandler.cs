using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Core.Entities;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.Mappers;

namespace YHTransporte.Application.Shipments.UseCases.GetShipment;

public sealed class GetShipmentHandler(IShipmentRepository repository)
{
    private readonly IShipmentRepository _respository = repository ?? throw new ArgumentNullException();

    public Task<IEnumerable<Shipment>> GetShipments(CancellationToken cancellationToken = default)
    => _respository.GetEverythingAsync(cancellationToken);

    public async Task<OneOf<Success<ShipmentDto>, NotFound>> GetShipmentByCode(string code, CancellationToken cancellationToken = default)
    {
        var shipment = (await _respository.GetByKeysAsync([code], cancellationToken)).FirstOrDefault();

        
        return shipment is not null ? new Success<ShipmentDto>(ShipmentMapper.ToValue(shipment)) :
        new NotFound();
    }
}