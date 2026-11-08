using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Driver.Views;

public partial class CreateDriverView : UserControl
{
    private readonly ResultManagerSubscription<CreateDriverViewModel> _subscriber;

    public CreateDriverView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }
}
