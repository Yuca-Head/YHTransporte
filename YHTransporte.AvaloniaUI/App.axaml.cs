using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Home.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Login.ViewModels;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.AvaloniaUI.Views;

namespace YHTransporte.AvaloniaUI;

public partial class App : Avalonia.Application
{


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindowVM = Program.Host is not null ? Program.Host.Services.GetRequiredService<MainViewModel>() :
        new();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowVM,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    
}