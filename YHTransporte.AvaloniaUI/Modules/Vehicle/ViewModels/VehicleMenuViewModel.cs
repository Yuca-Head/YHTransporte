using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class VehicleMenuViewModel : ViewModelBase
{
    public VehicleMenuViewModel(VehicleEditorViewModel vehicleEditorViewModel, VehicleListSectionViewModel listView)
    {
        _vehicleEditorViewModel = vehicleEditorViewModel;
        _listSection = listView;

        CurrentView = _listSection;

        _listSection.SelectedVehicleChanged += () =>
        {
            if(_listSection.SelectedVehicle is null)
                return;

            _vehicleEditorViewModel.Vehicle = _listSection.SelectedVehicle;

            CurrentView = _vehicleEditorViewModel;
        };

        _vehicleEditorViewModel.ViewIsClosed += () => CurrentView = _listSection;
    }

    [ObservableProperty]
    public partial ViewModelBase CurrentView { get; private set; }

    private readonly VehicleEditorViewModel _vehicleEditorViewModel;


    private readonly VehicleListSectionViewModel _listSection;
}
