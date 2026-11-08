using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class VehicleListSectionViewModel : ViewModelBase
{
    public VehicleListSectionViewModel(CreateVehicleViewModel vm, VehicleContext context)
    {
        CreateVehicleList = vm;
        _context = context;

        VehicleList = [];
        UpdateList();

        _context.VehiclesChanged += UpdateList;
    }

    private readonly VehicleContext _context;

    public event Action? SelectedVehicleChanged;

    // CAMBIO: antes era OrderCode? (marcador de posición); ahora es el vehículo real.
    [ObservableProperty]
    public partial VehicleDto? SelectedVehicle { get; set; }

    [ObservableProperty]
    public partial string SearchText {get; set;} = "";

    public void SelectedVehicleHasChanged()
    {
        SelectedVehicleChanged?.Invoke();
    }

    public ObservableCollection<VehicleDto> VehicleList { get; }


    [ObservableProperty]
    public partial CreateVehicleViewModel CreateVehicleList { get; set; }

    [RelayCommand]
    public void OpenCreator()
    => CreateVehicleList.IsOpen = true;

    partial void OnSearchTextChanged(string value)
    => UpdateList();

    private void UpdateList()
    {
        string search = SearchText.Trim();

        VehicleList.Clear();

        foreach(var vehicle in _context.Vehicles.Where(x => search.Length == 0 ||
        x.Plate.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        x.Description.Contains(search, StringComparison.OrdinalIgnoreCase)))
            VehicleList.Add(vehicle);
    }
}
