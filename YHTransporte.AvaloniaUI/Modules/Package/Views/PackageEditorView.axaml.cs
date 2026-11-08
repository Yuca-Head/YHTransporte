using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Package.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Package.Views;

public partial class PackageEditorView : UserControl
{
    private readonly ResultManagerSubscription<PackageEditorViewModel> _subscriber;

    public PackageEditorView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
