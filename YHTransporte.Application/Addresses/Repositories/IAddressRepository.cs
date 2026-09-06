using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.Repositories;

public interface IAddressRepository : IRepository<int, Address>
{
    Task<IEnumerable<Municipality>> GetMunicipalitiesByIdsAsync(IEnumerable<int> id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Department>> GetDepartmentsByIdsAsync(IEnumerable<int> id, CancellationToken cancellationToken = default);
    Task AddDepartmentsAsync(IEnumerable<Department> department, CancellationToken cancellationToken = default);
    Task AddMunicipalitiesAsync(IEnumerable<Municipality> municipality, CancellationToken cancellationToken = default);

    Task<IEnumerable<Department>> GetDepartmentsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task<IEnumerable<Municipality>> GetMunicipalitiesByNameAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task<IEnumerable<Address>> GetAddressesByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}