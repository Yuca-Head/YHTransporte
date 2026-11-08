using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.Mappers;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;

namespace YHTransporte.Application.Deliveries.UseCases.GetVehicle;

public sealed class GetVehicleHandler(IDeliveryRepository repository)
{
    private readonly IDeliveryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<VehicleDto>> GetVehiclesAsync(CancellationToken cancellationToken = default)
    => await _repository.GetVehicles(cancellationToken).Select(VehicleMapper.ToValue).ToListAsync(cancellationToken);

    public async Task<OneOf<Success<VehicleDto>, VehicleNotFound>> GetVehicleAsync(int id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _repository.GetVehicleAsync(id, cancellationToken);

        return vehicle is null ? new VehicleNotFound(id) : new Success<VehicleDto>(VehicleMapper.ToValue(vehicle));
    }

    public async Task<IEnumerable<ShipmentHistoryDto>> GetVehicleHistoryAsync(int id, CancellationToken cancellationToken = default)
    => await _repository.GetVehicleHistory(id, cancellationToken).ToListAsync(cancellationToken);
}
