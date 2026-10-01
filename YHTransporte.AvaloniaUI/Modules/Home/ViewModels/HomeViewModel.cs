using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Shipment.Views;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.Views;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel(ThirdPartyMenuViewModel thirdPartyMenu,DashboardMenuViewModel dashboardMenu,
    CargoMenuViewModel cargoMenu, ShipmentMenuViewModel shipmentMenu, 
    DriverMenuViewModel driverMenu, VehicleMenuViewModel vehicleMenu)
    {
        _thirdPartyMenu = thirdPartyMenu;
        _dashboardMenu = dashboardMenu;
        _cargoMenu = cargoMenu;
        _shipmentMenu = shipmentMenu;
        _driverMenu = driverMenu;
        _vehicleMenu = vehicleMenu;
        CurrentView = _dashboardMenu;
    }

    private readonly ThirdPartyMenuViewModel _thirdPartyMenu;
    private readonly DashboardMenuViewModel _dashboardMenu;
    private readonly CargoMenuViewModel _cargoMenu;
    private readonly ShipmentMenuViewModel _shipmentMenu;
    private readonly DriverMenuViewModel _driverMenu;
    private readonly VehicleMenuViewModel _vehicleMenu;

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}

    [RelayCommand]
    public void SetThirdPartyMenu()
    => ChangeView(_thirdPartyMenu);

    [RelayCommand]
    public void SetDashboardMenu()
    => ChangeView(_dashboardMenu);
    
    [RelayCommand]
    public void SetCargoMenu()
    => ChangeView(_cargoMenu);

    [RelayCommand]
    public void SetShipmentMenu()
    => ChangeView(_shipmentMenu);

    [RelayCommand]
    public void SetDriverMenu()
    => ChangeView(_driverMenu);

    [RelayCommand]
    public void SetVehicleMenu()
    => ChangeView(_vehicleMenu);
    private void ChangeView(ViewModelBase viewModel)
    {
        if (CurrentView! != viewModel)
            CurrentView = viewModel;
    }
}