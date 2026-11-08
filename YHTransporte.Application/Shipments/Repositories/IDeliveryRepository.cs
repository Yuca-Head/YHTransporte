using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Shipments.Dtos;
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


    /// <summary>Guarda placa y descripción del vehículo (procedimiento UpdateVehicle).</summary>
    Task UpdateVehicle(Vehicle vehicle, CancellationToken cancellationToken = default);

    /// <summary>Guarda los nombres del conductor, buscándolo por su código (procedimiento UpdateDriver).</summary>
    Task UpdateDriver(Driver driver, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si ya existe un vehículo con esa placa (sin importar mayúsculas ni espacios a los lados).
    /// </summary>
    /// <param name="excludedVehicleId">Vehículo que no se toma en cuenta (el que se está modificando).</param>
    Task<bool> PlateExists(string plate, int? excludedVehicleId = null, CancellationToken cancellationToken = default);

    /// <summary>Envíos en los que ha participado el vehículo (vista vw_ShipmentHistory), más recientes primero.</summary>
    IAsyncEnumerable<ShipmentHistoryDto> GetVehicleHistory(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>Envíos en los que ha participado el conductor (vista vw_ShipmentHistory), más recientes primero.</summary>
    IAsyncEnumerable<ShipmentHistoryDto> GetDriverHistory(string driverCode, CancellationToken cancellationToken = default);

}