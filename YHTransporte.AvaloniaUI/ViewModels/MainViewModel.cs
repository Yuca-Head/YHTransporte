using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.Modules.Home.ViewModels;
using YHTransporte.AvaloniaUI.Modules.Login.ViewModels;

namespace YHTransporte.AvaloniaUI.ViewModels;

public partial class MainViewModel: ViewModelBase
{

    public MainViewModel(LoginViewModel login, HomeViewModel home)
    {
        _login = login;
        _home = home;
        CurrentView = _login;

        _login.AuthenticatedUser += (_,_) => ViewToHome();

        #if DEBUG
            ViewToHome();
        #endif
    }

    internal MainViewModel(){}
    private readonly LoginViewModel _login;
    private readonly HomeViewModel _home;
    
    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; set;}

    [RelayCommand]
    private void ViewToLogin()
    => ChangeView(_login);

    [RelayCommand]
    private void ViewToHome()
    => ChangeView(_home);

    private void ChangeView(ViewModelBase viewModel)
    {
        if (CurrentView! != viewModel)
            CurrentView = viewModel;
    }
}
