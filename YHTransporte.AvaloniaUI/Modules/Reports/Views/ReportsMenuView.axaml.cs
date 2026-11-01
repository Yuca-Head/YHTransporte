using Avalonia.Controls;
using Avalonia.Interactivity;
using YHTransporte.AvaloniaUI.Modules.Reports.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Reports.Views;

public partial class ReportsMenuView : UserControl
{
    public ReportsMenuView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Genera el primer reporte al abrir la pantalla; InitializeAsync no lanza (captura sus errores).
        if (DataContext is ReportsMenuViewModel vm)
            await vm.InitializeAsync();
    }
}
