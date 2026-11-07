using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.Views;

public partial class OrderListSectionView : UserControl
{
    public OrderListSectionView()
    {
        InitializeComponent();
    }

    private void Order_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if(this.DataContext is OrderListSectionViewModel vm)
        {
            vm.SelectedOrderHasChanged();
        }
    }
}