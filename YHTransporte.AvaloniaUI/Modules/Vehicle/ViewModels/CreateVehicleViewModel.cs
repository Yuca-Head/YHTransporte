using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class CreateVehicleViewModel : ViewModelBase
{
    [ObservableProperty]
    private OrderCode? order;

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [RelayCommand]
    private void Close() => IsOpen = false;
}