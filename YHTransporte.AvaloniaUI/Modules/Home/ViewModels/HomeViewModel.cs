using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.Modules.Cargo.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Order.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Order.Views;
using YHTransporte.AvaloniaUI.Modules.Reports.ViewModels;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Vehicle.Views;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel(ThirdPartyMenuViewModel thirdPartyMenu,DashboardMenuViewModel dashboardMenu,
    CargoMenuViewModel cargoMenu, OrderMenuViewModel orderMenu, 
    DriverMenuViewModel driverMenu, VehicleMenuViewModel vehicleMenu, ReportsMenuViewModel reportsMenu)
    {
        _thirdPartyMenu = thirdPartyMenu;
        _dashboardMenu = dashboardMenu;
        _cargoMenu = cargoMenu;
        _orderMenu = orderMenu;
        _driverMenu = driverMenu;
        _vehicleMenu = vehicleMenu;
        _reportsMenu = reportsMenu;
        CurrentView = _dashboardMenu;
    }

    private readonly ThirdPartyMenuViewModel _thirdPartyMenu;
    private readonly DashboardMenuViewModel _dashboardMenu;
    private readonly CargoMenuViewModel _cargoMenu;
    private readonly OrderMenuViewModel _orderMenu;
    private readonly DriverMenuViewModel _driverMenu;
    private readonly VehicleMenuViewModel _vehicleMenu;
    private readonly ReportsMenuViewModel _reportsMenu;

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
    public void SetOrderMenu()
    => ChangeView(_orderMenu);

    [RelayCommand]
    public void SetDriverMenu()
    => ChangeView(_driverMenu);

    [RelayCommand]
    public void SetVehicleMenu()
    => ChangeView(_vehicleMenu);
    [RelayCommand]
    public void SetReportsMenu()
    => ChangeView(_reportsMenu);
    private void ChangeView(ViewModelBase viewModel)
    {
        if (CurrentView! != viewModel)
            CurrentView = viewModel;
    }
}