namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;

public sealed record ThirdPartyAddressSqlDto(int IdThirdParty, int Id, string Details, 
int IdMunicipality, string MunicipalityName, int DepartmentId, string DepartmentName);