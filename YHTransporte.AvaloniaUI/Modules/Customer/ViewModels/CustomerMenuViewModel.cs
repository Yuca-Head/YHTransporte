using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;

public partial class CustomerMenuViewModel(CreateCustomerViewModel createCustomer) : ViewModelBase
{
    public CreateCustomerViewModel CreateCustomer {get;} = createCustomer;
    
    [RelayCommand]
    private void OpenCustomerPopUp()
    => CreateCustomer.IsOpen = true;

    [RelayCommand]
    private void CloseCustomerPopUp()
    => CreateCustomer.IsOpen = false;
}