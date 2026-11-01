using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.Application.Orders.UseCases.CreateOrder;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class OrderListSectionViewModel : ViewModelBase
{

    public OrderListSectionViewModel(CreateOrderViewModel ordercreator)
    {
        var uno = new OrderCode(OrderCode.Generate("0001-311026%"));
        var two = new OrderCode(OrderCode.Generate(uno.Code));
        OrderList = [uno, two, new (OrderCode.Generate(two.Code))];
        Creator = ordercreator;
    }
    public event Action? SelectedOrderChanged;
    
    
    [ObservableProperty]
    public partial CreateOrderViewModel Creator {get; private set;}

    [ObservableProperty]
    public partial CreateOrderCommand? SelectedOrder {get; set;}

    public void SelectedOrderHasChanged()
    {
        SelectedOrderChanged?.Invoke();
    }

    public ObservableCollection<OrderCode> OrderList {get;}
}