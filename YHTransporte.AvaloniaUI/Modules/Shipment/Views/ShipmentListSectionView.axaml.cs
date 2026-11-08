using Avalonia.Controls;
using Avalonia.Input;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.Views;

public partial class ShipmentListSectionView : UserControl
{
    public ShipmentListSectionView()
    {
        InitializeComponent();
    }

    private void Shipment_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if(this.DataContext is ShipmentListSectionViewModel vm)
            vm.SelectedShipmentHasChanged();
    }
}
