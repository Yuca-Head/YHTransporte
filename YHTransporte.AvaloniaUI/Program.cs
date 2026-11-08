using Avalonia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneOf.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.Application.Addresses.UseCases.GetAddress;
using YHTransporte.Application.Deliveries.UseCases.CreateDriver;
using YHTransporte.Application.Deliveries.UseCases.CreateVehicle;
using YHTransporte.Application.Deliveries.UseCases.GetDriver;
using YHTransporte.Application.Deliveries.UseCases.GetVehicle;
using YHTransporte.Application.Deliveries.UseCases.ModifyDriver;
using YHTransporte.Application.Deliveries.UseCases.ModifyVehicle;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Orders.UseCases.CreateOrder;
using YHTransporte.Application.Orders.UseCases.GetOrder;
using YHTransporte.Application.Orders.UseCases.UpdateOrder;
using YHTransporte.Application.Reports;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.UseCases.AddPackage;
using YHTransporte.Application.Shipments.UseCases.CreateShipment;
using YHTransporte.Application.Shipments.UseCases.GetShipment;
using YHTransporte.Application.Shipments.UseCases.ModifyPackage;
using YHTransporte.Application.Shipments.UseCases.ModifyShipment;
using YHTransporte.Application.Shipments.UseCases.UpdateShipment;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.Application.ThirdParties.UseCases.CreateSupplierProduct;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdPartyAddress;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.Application.ThirdParties.UseCases.ModifySupplierProduct;
using YHTransporte.AvaloniaUI.Modules.Address.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Home.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Login.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Order.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Package.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Reports.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.Extra;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.AvaloniaUI.Views;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Orders;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Reports;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shipments;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties;

namespace YHTransporte.AvaloniaUI;

public sealed class Program
{
    public static IHost Host { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args) 
    {

        var builder = Microsoft.Extensions.Hosting.Host
        .CreateApplicationBuilder(args);


        ConfigureServices(builder.Services);

        builder.Configuration.AddUserSecrets<Program>().Build();

        Host = builder.Build();

        var contexts =  Host.Services.GetServices<IInitiableContext>();

        foreach(var c in contexts)
        {   
            await c.Init();
        }
        
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureServices(IServiceCollection services)
    {
        //ViewModels
        services.AddTransient<HomeViewModel>(); //Navbar
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<ThirdPartyMenuViewModel>();
        services.AddTransient<DashboardMenuViewModel>();
        services.AddTransient<CargoMenuViewModel>();
        services.AddTransient<OrderMenuViewModel>();
        services.AddTransient<CreateThirdPartyViewModel>();
        services.AddTransient<DriverMenuViewModel>();
        services.AddTransient<VehicleMenuViewModel>();
        services.AddTransient<ThirdPartyListSectionViewModel>();
        services.AddTransient<ThirdPartyEditorMenuViewModel>();
        services.AddTransient<CreateMunicipalityViewModel>();
        services.AddTransient<CreateDepartmentViewModel>();
        services.AddTransient<OrderListSectionViewModel>();
        services.AddTransient<OrderEditorViewModel>();
        services.AddTransient<CreateOrderViewModel>();
        services.AddTransient<ReportsMenuViewModel>();
        services.AddTransient<DashboardListSectionViewModel>();
        services.AddTransient<VehicleEditorViewModel>();
        services.AddTransient<VehicleListSectionViewModel>();
        services.AddTransient<CreateVehicleViewModel>();
        // Envíos (dentro del editor de órdenes)
        services.AddTransient<ShipmentMenuViewModel>();
        services.AddTransient<ShipmentListSectionViewModel>();
        services.AddTransient<CreateShipmentViewModel>();
        services.AddTransient<ShipmentEditorViewModel>();
        // Paquetes (dentro del editor de envíos)
        services.AddTransient<PackageMenuViewModel>();
        services.AddTransient<PackageListSectionViewModel>();
        services.AddTransient<CreatePackageViewModel>();
        services.AddTransient<PackageEditorViewModel>();
        // Conductores
        services.AddTransient<DriverListSectionViewModel>();
        services.AddTransient<CreateDriverViewModel>();
        services.AddTransient<DriverEditorViewModel>();

        //Contexts
        RegisterInitiableContext<ThirdPartyContext>(services);
        RegisterInitiableContext<AddressContext>(services);
        RegisterInitiableContext<OrderContext>(services);
        RegisterInitiableContext<ShipmentContext>(services);
        RegisterInitiableContext<VehicleContext>(services);
        RegisterInitiableContext<DriverContext>(services);

        //Windows
        services.AddTransient<MainWindow>();

        //UseCases
        services.AddSingleton<CreateThirdPartyHandler>();
        services.AddSingleton<GetThirdPartyHandler>();
        services.AddSingleton<CreateAddressHandler>();
        services.AddSingleton<GetAddressHandler>();
        services.AddSingleton<CreateThirdPartyAddressHandler>();
        services.AddSingleton<CreateOrderHandler>();
        services.AddSingleton<GetOrderHandler>();
        services.AddSingleton<UpdateOrderHandler>();
        services.AddSingleton<GetShipmentHandler>();
        services.AddSingleton<CreateShipmentHandler>();
        services.AddSingleton<UpdateShipmentHandler>();
        services.AddSingleton<ModifyShipmentHandler>();
        services.AddSingleton<AddPackageHandler>();
        services.AddSingleton<ModifyPackageHandler>();
        services.AddSingleton<CreateVehicleHandler>();
        services.AddSingleton<ModifyVehicleHandler>();
        services.AddSingleton<GetVehicleHandler>();
        services.AddSingleton<CreateDriverHandler>();
        services.AddSingleton<ModifyDriverHandler>();
        services.AddSingleton<GetDriverHandler>();
        services.AddSingleton<CreateSupplierProductHandler>();
        services.AddSingleton<ModifySupplierProductHandler>();

        //Repositories
        services.AddSingleton<IThirdPartyRepository, SqlServerThirdPartyRepository>();
        services.AddSingleton<IAddressRepository, SqlServerAddressRepository>();
        services.AddSingleton<IOrderRepository, SqlServerOrderRepository>();
        services.AddSingleton<IShipmentReportRepository, EfShipmentReportRepository>();
        services.AddSingleton<IShipmentRepository, SqlServerShipmentRepository>();
        services.AddSingleton<IDeliveryRepository, SqlServerDeliveryRepository>();

        //dbRelated
        services.AddSingleton<DbConnectionFactory>();
        services.AddDbContextFactory<YHTransporteDbContext>((sp, options) =>
        options.UseSqlServer(sp.GetRequiredService<DbConnectionFactory>().ConnectionString));

        //Others
        services.AddTransient<ThirdPartyAddressCreator>();
        services.AddTransient<ThirdPartyProductManager>();

    }
            

    private static void RegisterInitiableContext<T>(IServiceCollection services)
    where T : class, IInitiableContext
    {
        services.AddSingleton<T>();
        services.AddSingleton<IInitiableContext>(sp => sp.GetRequiredService<T>());
    }
}