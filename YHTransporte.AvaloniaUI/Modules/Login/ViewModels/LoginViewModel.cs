using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.AvaloniaUI.Modules.Login.Models;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Login.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [RelayCommand]
    private static void CloseApp()
    => WeakReferenceMessenger.Default.Send(new CloseApplicationMessage());

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage {get; set;} = "";
    public event EventHandler? AuthenticatedUser;

    [ObservableProperty]
    public partial LogUserModel User {get; set;} = new("","");
    
    //Here should be everything related to the users connections...
    [RelayCommand]
    private void AuthenticateUser()
    {
        //Super validation here...
        if(User.User == "YucaHead" && User.Password == "Contraseña")
            AuthenticatedUser?.Invoke(this, EventArgs.Empty);
        else   
            ErrorMessage = "Usuario o contraseña no válido";
    }
}