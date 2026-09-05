using Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Home.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Login.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.Views;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.AvaloniaUI.Views;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties;

namespace YHTransporte.AvaloniaUI;

sealed class Program
{
    public static IHost Host { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
    {

        var builder = Microsoft.Extensions.Hosting.Host
        .CreateApplicationBuilder(args);


        ConfigureServices(builder.Services);

        builder.Configuration.AddUserSecrets<Program>().Build();

        Host = builder.Build();

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
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<CustomerMenuViewModel>();
        services.AddTransient<DashboardMenuViewModel>();
        services.AddTransient<CargoMenuViewModel>();
        services.AddTransient<ShipmentMenuViewModel>();
        services.AddTransient<CreateCustomerViewModel>();

        //Windows
        services.AddTransient<MainWindow>();

        //UseCases
        services.AddSingleton<CreateThirdPartyHandler>();
        services.AddSingleton<CreateThirdPartyValidator>();


        //Repositories
        services.AddSingleton<IThirdPartyRepository, SqlServerThirdPartyRepository>();



        //Others
        services.AddSingleton<DbConnectionFactory>();

    }
            
}
