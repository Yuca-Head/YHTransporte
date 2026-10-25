using Microsoft.EntityFrameworkCore;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

public sealed class YHTransporteDbContext(DbContextOptions<YHTransporteDbContext> options)
    : DbContext(options)
{
    internal DbSet<DepartmentSqlDto> Departments => Set<DepartmentSqlDto>();
    internal DbSet<ThirdPartySqlDto> ThirdParties => Set<ThirdPartySqlDto>();
    internal DbSet<ThirdPartyAddressSqlRow> ThirdPartyAddressesTable => Set<ThirdPartyAddressSqlRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DepartmentSqlDto>(e =>
        {
            e.ToTable("Departments");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ThirdPartySqlDto>(e =>
        {
            e.ToTable("ThirdParties");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ThirdPartyAddressSqlRow>(e =>
        {
            e.ToTable("ThirdPartiesAddresses");
            e.HasKey(row => new {row.IdThirdParty, row.IdAddress});
        });
    }
}
