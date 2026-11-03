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
    /// <param name="driverCode">Code of the driver to update.</param>
    /// <returns></returns>
    Task ChangeDriverState(string driverCode, CancellationToken cancellationToken = default);


    /// <summary>
    /// Changes vehicle's "In Use" Property into the opposite value.
    /// </summary>
    /// <param name="vehicleId">Id of the vehicle to update.</param>
    /// <returns></returns>
    Task ChangeVehicleState(int vehicleId, CancellationToken cancellationToken = default);

}