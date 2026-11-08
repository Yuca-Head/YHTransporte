using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Package.ViewModels;

/// <summary>
/// Menú de paquetes de un envío. Vive dentro de ShipmentEditorViewModel:
/// alterna entre la lista de paquetes y el editor mínimo de un paquete.
/// </summary>
public partial class PackageMenuViewModel : ViewModelBase
{
    public PackageMenuViewModel(PackageListSectionViewModel listSection, PackageEditorViewModel editor)
    {
        _listSection = listSection;
        _editor = editor;

        CurrentView = _listSection;

        _listSection.SelectedPackageChanged += () =>
        {
            if(_listSection.SelectedPackage is null)
                return;

            _editor.Package = _listSection.SelectedPackage;
            CurrentView = _editor;
        };

        _editor.ViewIsClosed += () => CurrentView = _listSection;
    }

    private readonly PackageListSectionViewModel _listSection;
    private readonly PackageEditorViewModel _editor;

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}

    [ObservableProperty]
    public partial ShipmentDto? Shipment {get; set;}

    [ObservableProperty]
    public partial OrderDto? Order {get; set;}

    partial void OnShipmentChanged(ShipmentDto? oldValue, ShipmentDto? newValue)
    {
        _listSection.Shipment = newValue;
        _editor.Shipment = newValue;

        if(oldValue?.Key != newValue?.Key)
        {
            Reset();
            return;
        }

        // Mismo envío refrescado: el editor toma la versión nueva de su paquete.
        if(_editor.Package is PackageDto current && newValue?.Packages.FirstOrDefault(x => x.Key == current.Key) is PackageDto updated)
            _editor.Package = updated;
    }

    partial void OnOrderChanged(OrderDto? value)
    {
        _listSection.Order = value;
        _editor.Order = value;
    }

    public void Reset()
    {
        _listSection.Creator.Close();
        _editor.Package = null;
        CurrentView = _listSection;
    }
}
