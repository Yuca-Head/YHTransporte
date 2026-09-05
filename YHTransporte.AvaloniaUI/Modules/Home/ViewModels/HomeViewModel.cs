using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.Views;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel(CustomerMenuViewModel customerMenu, DashboardMenuViewModel dashboardMenu,
    CargoMenuViewModel cargoMenu, ShipmentMenuViewModel shipmentMenu)
    {
        _customerMenu = customerMenu;
        _dashboardMenu = dashboardMenu;
        _cargoMenu = cargoMenu;
        _shipmentMenu = shipmentMenu;
        CurrentView = _dashboardMenu;
    }

    private readonly CustomerMenuViewModel _customerMenu;
    private readonly DashboardMenuViewModel _dashboardMenu;
    private readonly CargoMenuViewModel _cargoMenu;
    private readonly ShipmentMenuViewModel _shipmentMenu;

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}

    [RelayCommand]
    public void SetCustomerMenu()
    => ChangeView(_customerMenu);

    [RelayCommand]
    public void SetDashboardMenu()
    => ChangeView(_dashboardMenu);
    
    [RelayCommand]
    public void SetCargoMenu()

    => ChangeView(_cargoMenu);

    [RelayCommand]
    public void SetShipmentMenu()
    => ChangeView(_shipmentMenu); 

    private void ChangeView(ViewModelBase viewModel)
    {
        if (CurrentView! != viewModel)
            CurrentView = viewModel;
    }
}