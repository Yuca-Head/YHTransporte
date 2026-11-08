using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.Views;

public partial class CreateVehicleView : UserControl
{
    private readonly ResultManagerSubscription<CreateVehicleViewModel> _subscriber;

    public CreateVehicleView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
