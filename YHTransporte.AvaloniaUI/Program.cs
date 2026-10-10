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
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdPartyAddress;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.AvaloniaUI.Modules.Address.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Home.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Login.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.Views;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.Extra;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.AvaloniaUI.Views;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Addresses;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
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
            await c.Init();

        var createUC = Host.Services.GetService<CreateAddressHandler>();
        
        //await createUC.HandleDepartment([new("Matagalpa", -1)]);
        //await createUC.HandleMunicipality([new("Ciudad Darío", 3)]);
        //await createUC.HandleAddress([new("De la venta \"La Bonicua\" dos cuadras abajo", 3)]);
        /*
        var tps = Host.Services.GetRequiredService<ThirdPartyContext>();
        var rp = Host.Services.GetRequiredService<GetThirdPartyHandler>();



        await rp.LoadThirdParties();

        //(await rp.LoadThirdParties()).ToList().ForEach(x => System.Console.WriteLine(x.Name));
        tps.Customers.ToList().ForEach(x => System.Console.WriteLine(x.Name));

        System.Console.WriteLine(tps.Customers.Count);
        */
        
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
        services.AddTransient<ShipmentMenuViewModel>();
        services.AddTransient<CreateThirdPartyViewModel>();
        services.AddTransient<DriverMenuViewModel>();
        services.AddTransient<VehicleMenuViewModel>();
        services.AddTransient<ThirdPartyListSectionViewModel>();
        services.AddTransient<ThirdPartyEditorMenuViewModel>();
        services.AddTransient<CreateMunicipalityViewModel>();
        services.AddTransient<CreateDepartmentViewModel>();

        //Contexts
        RegisterInitiableContext<ThirdPartyContext>(services);
        RegisterInitiableContext<AddressContext>(services);

        //Windows
        services.AddTransient<MainWindow>();

        //UseCases
        services.AddSingleton<CreateThirdPartyHandler>();
        services.AddSingleton<GetThirdPartyHandler>();
        services.AddSingleton<CreateAddressHandler>();
        services.AddSingleton<GetAddressHandler>();
        services.AddSingleton<CreateThirdPartyAddressHandler>();


        //Repositories
        services.AddSingleton<IThirdPartyRepository, SqlServerThirdPartyRepository>();
        services.AddSingleton<IAddressRepository, SqlServerAddressRepository>();


        //dbRelated
        services.AddSingleton<DbConnectionFactory>();
        services.AddDbContextFactory<YHTransporteDbContext>((sp, options) =>
        options.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString("AzureConnection")));

        //Others
        services.AddTransient<ThirdPartyAddressCreator>();

    }
            

    private static void RegisterInitiableContext<T>(IServiceCollection services)
    where T : class, IInitiableContext
    {
        services.AddSingleton<T>();
        services.AddSingleton<IInitiableContext>(sp => sp.GetRequiredService<T>());
    }
}