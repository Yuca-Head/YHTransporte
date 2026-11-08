using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.AvaloniaUI.Modules.Order.Models;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class OrderMenuViewModel : ViewModelBase
{
    public OrderMenuViewModel(CreateOrderViewModel createOrderViewModel, OrderListSectionViewModel listView,
    OrderEditorViewModel editor)
    {
        _createOrderView = createOrderViewModel;
        _listSection = listView;
        _editor = editor;

        CurrentView = _listSection;
        

        _listSection.SelectedOrderChanged += () =>
        {
            if(_listSection.SelectedOrder is not OrderDetailsVM selected)
                return;

            _editor.Order = selected.Dto;
            CurrentView = _editor;
        };

        _editor.ViewIsClosed += () => CurrentView = _listSection;
    }

    [ObservableProperty]
    public partial ViewModelBase CurrentView { get; private set; }
    private readonly CreateOrderViewModel _createOrderView;

    private readonly OrderEditorViewModel _editor;

    private readonly OrderListSectionViewModel _listSection;


    [ObservableProperty]
    public partial string ResultMessage { get; private set; } = "";

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool IsOpen {get; set;}
    
}