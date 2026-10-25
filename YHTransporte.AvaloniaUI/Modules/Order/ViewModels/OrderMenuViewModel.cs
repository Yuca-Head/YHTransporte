using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class OrderMenuViewModel : ViewModelBase
{
    public OrderMenuViewModel(CreateOrderViewModel createOrderViewModel, OrderListSectionViewModel listView)
    {
        _createOrderView = createOrderViewModel;
        _listSection = listView;

        CurrentView = _listSection;
        

        _listSection.SelectedOrderChanged += () =>
        {

            CurrentView = _createOrderView;
        };
    }

    [ObservableProperty]
    public partial ViewModelBase CurrentView { get; private set; }
    private readonly CreateOrderViewModel _createOrderView;



    private readonly OrderListSectionViewModel _listSection;


    [ObservableProperty]
    public partial string ResultMessage { get; private set; } = "";

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool IsOpen {get; set;}
}