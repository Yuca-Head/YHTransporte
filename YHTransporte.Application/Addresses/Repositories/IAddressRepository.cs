using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.Repositories;

public interface IAddressRepository : IRepository<int, Address>
{
    Task<Municipality> GetMunicipalityByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Department> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddDepartmentAsync(Department department, CancellationToken cancellationToken = default);
    Task AddMunicipalityAsync(Municipality municipality, CancellationToken cancellationToken = default);
}