namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

public sealed record ThirdPartySqlDto(int Id, string Name, bool IsSupplier, bool IsCustomer);