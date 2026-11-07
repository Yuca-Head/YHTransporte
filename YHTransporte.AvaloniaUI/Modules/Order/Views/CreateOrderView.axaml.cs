using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Order.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Order.Views;

public partial class CreateOrderView : UserControl
{

    private readonly ResultManagerSubscription<CreateOrderViewModel> _subscriber;
    public CreateOrderView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }


}