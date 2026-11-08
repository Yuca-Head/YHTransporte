using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Order.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Order.Views;

public partial class OrderEditorView : UserControl
{
    private readonly ResultManagerSubscription<OrderEditorViewModel> _subscriber;

    public OrderEditorView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }

}
