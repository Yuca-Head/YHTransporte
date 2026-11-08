using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.Views;

public partial class VehicleEditorView : UserControl
{
    private readonly ResultManagerSubscription<VehicleEditorViewModel> _subscriber;

    public VehicleEditorView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
