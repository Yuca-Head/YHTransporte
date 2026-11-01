using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.Repositories;

public interface IDeliveryRepository : IRepository
{
    Task AddVehicle(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task AddDriver(Driver driver, CancellationToken cancellationToken = default);

    IAsyncEnumerable<Driver> GetDrivers(CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<Vehicle> GetVehicles(CancellationToken cancellationToken = default);

    Task<Driver?> GetDriverAsync(string driverCode, CancellationToken cancellationToken = default);

    Task<Vehicle?> GetVehicleAsync(int vehicleId, CancellationToken cancellationToken = default);

    Task<bool> VehicleExists(int vehicleId, CancellationToken cancellationToken = default);

    Task<bool> DriverExists(string driverCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes driver's "In Use" Property into the opposite value.
    /// </summary>
    /// <returns></returns>
    Task ChangeDriverState();

    
    /// <summary>
    /// Changes vehicle's "In Use" Property into the opposite value.
    /// </summary>
    /// <returns></returns>
    Task ChangeVehicleState();

}