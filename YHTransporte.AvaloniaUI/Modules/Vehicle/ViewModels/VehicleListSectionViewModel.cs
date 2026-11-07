using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class VehicleListSectionViewModel : ViewModelBase
{
    public VehicleListSectionViewModel(CreateVehicleViewModel vm)
    {
        CreateVehicleList = vm;

        VehicleList = [];
    }
    public event Action? SelectedVehicleChanged;

    [ObservableProperty]
    public partial OrderCode? SelectedVehicle { get; set; }

    public void SelectedVehicleHasChanged()
    {
        SelectedVehicleChanged?.Invoke();
    }

    public ObservableCollection<OrderCode> VehicleList { get; }


    [ObservableProperty]
    public partial CreateVehicleViewModel CreateVehicleList { get; set; }

    [RelayCommand]
    public void OpenCreator()
    => CreateVehicleList.IsOpen = true;

}