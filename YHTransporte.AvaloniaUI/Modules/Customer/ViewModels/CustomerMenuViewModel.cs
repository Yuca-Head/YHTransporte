using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;

public partial class CustomerMenuViewModel(CreateCustomerViewModel createCustomer) : ViewModelBase
{
    public CreateCustomerViewModel CreateCustomer {get;} = createCustomer;

    [ObservableProperty]
    public partial bool IsAddAddressOptionOpen {get; set;}
    
    [RelayCommand]
    private void OpenCustomerPopUp()
    => CreateCustomer.IsOpen = true;

    [RelayCommand]
    private void CloseCustomerPopUp()
    => CreateCustomer.IsOpen = false;

    [RelayCommand]
    private void ShowAddAddress()
    => IsAddAddressOptionOpen = true;

    [RelayCommand]
    private void CancelAddAddress()
    => IsAddAddressOptionOpen = false;
}