using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.Views;

public partial class CreateShipmentView : UserControl
{
    private readonly ResultManagerSubscription<CreateShipmentViewModel> _subscriber;

    public CreateShipmentView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }

}
