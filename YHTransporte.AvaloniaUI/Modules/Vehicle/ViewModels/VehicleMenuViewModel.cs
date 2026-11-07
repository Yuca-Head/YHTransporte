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
            _vehicleEditorViewModel.Order = _listSection.SelectedVehicle;

            CurrentView = _vehicleEditorViewModel;
        };

        _vehicleEditorViewModel.ViewIsClosed += () => CurrentView = _listSection;
    }

    [ObservableProperty]
    public partial ViewModelBase CurrentView { get; private set; }

    private readonly VehicleEditorViewModel _vehicleEditorViewModel;


    private readonly VehicleListSectionViewModel _listSection;


    [ObservableProperty]
    public partial string ResultMessage { get; private set; } = "";

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    private bool _isOpen;
}