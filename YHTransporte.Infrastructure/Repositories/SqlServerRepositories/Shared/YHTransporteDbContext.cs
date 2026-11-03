using Microsoft.EntityFrameworkCore;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses.Dtos;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Reports.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
public sealed class YHTransporteDbContext(DbContextOptions<YHTransporteDbContext> options)
    : DbContext(options)
{
    internal DbSet<DepartmentSqlDto> Departments => Set<DepartmentSqlDto>();
    internal DbSet<ThirdPartySqlDto> ThirdParties => Set<ThirdPartySqlDto>();
    internal DbSet<ThirdPartyAddressSqlRow> ThirdPartyAddressesTable => Set<ThirdPartyAddressSqlRow>();
    internal DbSet<ProductSqlRow> Products => Set<ProductSqlRow>();
    internal DbSet<SupplierProductSqlRow> SupplierProducts => Set<SupplierProductSqlRow>();

    // Entregable (EF Core + reportes): órdenes, envíos y paquetes con sus relaciones
    internal DbSet<OrderStatusRecord> OrderStatuses => Set<OrderStatusRecord>();
    internal DbSet<ShipmentStatusRecord> ShipmentStatuses => Set<ShipmentStatusRecord>();
    internal DbSet<OrderRecord> Orders => Set<OrderRecord>();
    internal DbSet<ShipmentRecord> Shipments => Set<ShipmentRecord>();
    internal DbSet<PackageRecord> Packages => Set<PackageRecord>();

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

        modelBuilder.Entity<ProductSqlRow>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SupplierProductSqlRow>(e =>
        {
            e.ToTable("SupplierProducts");
            e.HasKey(x => new { x.IdProduct, x.IdSupplier });
        });

        modelBuilder.Entity<OrderStatusRecord>(e =>
        {
            e.ToTable("OrderStatuses");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ShipmentStatusRecord>(e =>
        {
            e.ToTable("ShipmentStatuses");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<OrderRecord>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.Code);

            // Orders.IdStatus -> OrderStatuses.Id
            e.HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.IdStatus);
        });

        modelBuilder.Entity<ShipmentRecord>(e =>
        {
            e.ToTable("Shipments");
            e.HasKey(x => x.Code);

            // Shipments.CodeOrder -> Orders.Code (una orden tiene muchos envíos)
            e.HasOne(x => x.Order)
            .WithMany(o => o.Shipments)
            .HasForeignKey(x => x.CodeOrder);

            // Shipments.IdStatus -> ShipmentStatuses.Id
            e.HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.IdStatus);
        });

        modelBuilder.Entity<PackageRecord>(e =>
        {
            e.ToTable("ShipmentsDetails");
            e.HasKey(x => new { x.ShipmentCode, x.Id });

            e.Property(x => x.ShipmentCode).HasColumnName("Code_Shipment");
            e.Property(x => x.WeightKg).HasColumnName("Weight_kg").HasPrecision(10, 2);

            // ShipmentsDetails.Code_Shipment -> Shipments.Code (un envío tiene muchos paquetes)
            e.HasOne(x => x.Shipment)
            .WithMany(s => s.Packages)
            .HasForeignKey(x => x.ShipmentCode);
        });
    }
}