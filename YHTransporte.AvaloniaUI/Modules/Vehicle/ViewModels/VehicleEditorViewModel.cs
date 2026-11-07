using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class VehicleEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private OrderCode? order;

    public event Action? ViewIsClosed;

    [RelayCommand]
    private void Exit()
    {
        Order = null;
        ViewIsClosed?.Invoke();
    }

}