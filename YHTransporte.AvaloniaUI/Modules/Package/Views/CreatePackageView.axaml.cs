using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Package.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Package.Views;

public partial class CreatePackageView : UserControl
{
    private readonly ResultManagerSubscription<CreatePackageViewModel> _subscriber;

    public CreatePackageView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
