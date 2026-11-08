using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.Views;

public partial class ShipmentEditorView : UserControl
{
    private readonly ResultManagerSubscription<ShipmentEditorViewModel> _subscriber;

    public ShipmentEditorView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
