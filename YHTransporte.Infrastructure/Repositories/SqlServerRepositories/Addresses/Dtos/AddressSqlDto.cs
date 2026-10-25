namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;

public sealed record AddressSqlDto(
int Id, string Details, int MunicipalityId, string MunicipalityName, 
int DepartmentId, string DepartmentName);