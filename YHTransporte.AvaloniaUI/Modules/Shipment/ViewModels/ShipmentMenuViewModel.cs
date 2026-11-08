using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;

/// <summary>
/// Menú de envíos. Solo vive dentro del editor de órdenes (OrderEditorViewModel):
/// muestra la lista de envíos de la orden o el editor del envío seleccionado.
/// </summary>
public partial class ShipmentMenuViewModel : ViewModelBase
{
    public ShipmentMenuViewModel(ShipmentListSectionViewModel listSection, ShipmentEditorViewModel editor)
    {
        _listSection = listSection;
        _editor = editor;

        CurrentView = _listSection;

        _listSection.SelectedShipmentChanged += () =>
        {
            if(_listSection.SelectedShipment is null)
                return;

            _editor.Shipment = _listSection.SelectedShipment;
            CurrentView = _editor;
        };

        _editor.ViewIsClosed += () => CurrentView = _listSection;
    }

    private readonly ShipmentListSectionViewModel _listSection;
    private readonly ShipmentEditorViewModel _editor;

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}

    [ObservableProperty]
    public partial OrderDto? Order {get; set;}

    partial void OnOrderChanged(OrderDto? oldValue, OrderDto? newValue)
    {
        _listSection.Order = newValue;
        _editor.Order = newValue;

        // Si se abrió otra orden se regresa a la lista.
        if(oldValue?.Code != newValue?.Code)
            Reset();
    }

    /// <summary>Vuelve a la lista y cierra el editor/creador abiertos.</summary>
    public void Reset()
    {
        _listSection.Creator.Close();
        _editor.Shipment = null;
        CurrentView = _listSection;
    }
}
