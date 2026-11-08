using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Driver.Views;

public partial class DriverEditorView : UserControl
{
    private readonly ResultManagerSubscription<DriverEditorViewModel> _subscriber;

    public DriverEditorView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
