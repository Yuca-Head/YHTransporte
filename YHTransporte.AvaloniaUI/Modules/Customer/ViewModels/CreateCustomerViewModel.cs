using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.AvaloniaUI.Modules.Customer.Models;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;

public partial class CreateCustomerViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial CreateCustomerModel NewCustomer{ get; set; }

    [RelayCommand]
    private async Task CreateCustomer()
    {
        
    }
}