using Avalonia.Controls;
using Avalonia.Input;
using YHTransporte.AvaloniaUI.Modules.Order.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.Views;

public partial class VehicleListSectionView : UserControl
{
    public VehicleListSectionView()
    {
        InitializeComponent();
    }
    private void Vehicle_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (this.DataContext is VehicleListSectionViewModel vm)
        {
            vm.SelectedVehicleHasChanged();
        }
    }
}