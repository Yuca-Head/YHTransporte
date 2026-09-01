using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public HomeViewModel(CustomerMenuViewModel customerMenu, DashboardMenuViewModel dashboardMenu)
    {
        _customerMenu = customerMenu;
        _dashboardMenu = dashboardMenu;
        CurrentView = _dashboardMenu;
    }

    private readonly CustomerMenuViewModel _customerMenu;
    private readonly DashboardMenuViewModel _dashboardMenu;

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}

    [RelayCommand]
    public void SetCustomerMenu()
    => ChangeView(_customerMenu);

    [RelayCommand]
    public void SetDashboardMenu()
    => ChangeView(_dashboardMenu);


    private void ChangeView(ViewModelBase viewModel)
    {
        if (CurrentView! != viewModel)
            CurrentView = viewModel;
    }
}