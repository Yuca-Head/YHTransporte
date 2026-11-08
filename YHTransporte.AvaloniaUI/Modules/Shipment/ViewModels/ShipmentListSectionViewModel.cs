using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;

public partial class ShipmentListSectionViewModel : ViewModelBase
{
    public ShipmentListSectionViewModel(ShipmentContext context, CreateShipmentViewModel creator)
    {
        _context = context;
        Creator = creator;

        _context.ShipmentsChanged += UpdateList;
    }

    private readonly ShipmentContext _context;

    public event Action? SelectedShipmentChanged;

    public CreateShipmentViewModel Creator {get;}

    public ObservableCollection<ShipmentDto> Shipments {get;} = [];

    [ObservableProperty]
    public partial ShipmentDto? SelectedShipment {get; set;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddShipments))]
    public partial OrderDto? Order {get; set;}

    [ObservableProperty]
    public partial string SearchText {get; set;} = "";

    /// <summary>Solo se agregan envíos mientras la orden está en espera (regla de CreateShipmentHandler).</summary>
    public bool CanAddShipments => Order?.Status is OrderStatuses.Pending;

    partial void OnOrderChanged(OrderDto? value)
    {
        Creator.Order = value;
        UpdateList();
    }

    partial void OnSearchTextChanged(string value)
    => UpdateList();

    public void SelectedShipmentHasChanged()
    => SelectedShipmentChanged?.Invoke();

    [RelayCommand]
    public void OpenCreator()
    => Creator.IsOpen = true;

    private void UpdateList()
    {
        Shipments.Clear();

        if(Order is null)
            return;

        var shipments = _context.GetByOrder(Order.Code)
        .Where(x => string.IsNullOrWhiteSpace(SearchText) ||
            x.Key.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) ||
            (x.Driver?.FullName.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) ?? false) ||
            (x.Vehicle?.Plate.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) ?? false))
        .OrderBy(x => x.Key);

        foreach(var shipment in shipments)
            Shipments.Add(shipment);
    }
}
