using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.Mappers;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Results;

namespace YHTransporte.Application.Deliveries.UseCases.GetDriver;

public sealed class GetDriverHandler(IDeliveryRepository repository)
{
    private readonly IDeliveryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<DriverDto>> GetDriversAsync(CancellationToken cancellationToken = default)
    => await _repository.GetDrivers(cancellationToken).Select(DriverMapper.ToValue).ToListAsync(cancellationToken);

    public async Task<OneOf<Success<DriverDto>, DriverNotFound>> GetDriverAsync(string code, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(code))
            return new DriverNotFound(code ?? "");

        var driver = await _repository.GetDriverAsync(code, cancellationToken);

        return driver is null ? new DriverNotFound(code) : new Success<DriverDto>(DriverMapper.ToValue(driver));
    }

    public async Task<IEnumerable<ShipmentHistoryDto>> GetDriverHistoryAsync(string code, CancellationToken cancellationToken = default)
    => await _repository.GetDriverHistory(code, cancellationToken).ToListAsync(cancellationToken);
}
