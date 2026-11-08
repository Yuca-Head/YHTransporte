using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.UseCases.ModifyShipment;
using YHTransporte.Application.Shipments.UseCases.UpdateShipment;
using YHTransporte.AvaloniaUI.Modules.Package.ViewModels;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;

/// <summary>
/// Editor de un envío: información general, cambio de estado (pendiente -> en camino -> entregado), cancelación,
/// edición de conductor/vehículo/fecha/descripción (solo mientras está pendiente) y el menú de paquetes.
/// </summary>
public partial class ShipmentEditorViewModel : ViewModelBase, IResultProvider
{
    public ShipmentEditorViewModel(UpdateShipmentHandler updateHandler, ModifyShipmentHandler modifyHandler,
    ShipmentContext shipmentContext, VehicleContext vehicleContext, DriverContext driverContext, PackageMenuViewModel packageMenu)
    {
        _updateHandler = updateHandler;
        _modifyHandler = modifyHandler;
        _shipmentContext = shipmentContext;
        _vehicleContext = vehicleContext;
        _driverContext = driverContext;
        PackageMenu = packageMenu;

        // Si el envío cambia (estado, paquetes, datos) se vuelve a leer desde el contexto.
        _shipmentContext.ShipmentsChanged += () =>
        {
            if(Shipment is not null && _shipmentContext.GetById(Shipment.Key) is ShipmentDto updated)
                Shipment = updated;
        };

        _vehicleContext.VehiclesChanged += () => UpdateVehicles(keepSelection: true);
        _driverContext.DriversChanged += () => UpdateDrivers(keepSelection: true);
    }

    private readonly UpdateShipmentHandler _updateHandler;
    private readonly ModifyShipmentHandler _modifyHandler;
    private readonly ShipmentContext _shipmentContext;
    private readonly VehicleContext _vehicleContext;
    private readonly DriverContext _driverContext;

    public event Action? ViewIsClosed;

    public PackageMenuViewModel PackageMenu {get;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit), nameof(CanAdvanceStatus), nameof(CanCancel), nameof(NextStatusText))]
    public partial ShipmentDto? Shipment {get; set;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit), nameof(CanAdvanceStatus), nameof(CanCancel))]
    public partial OrderDto? Order {get; set;}

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    // ── Campos editables ──
    public ObservableCollection<VehicleDto> Vehicles {get;} = [];
    public ObservableCollection<DriverDto> Drivers {get;} = [];

    [ObservableProperty]
    public partial VehicleDto? SelectedVehicle {get; set;}

    [ObservableProperty]
    public partial DriverDto? SelectedDriver {get; set;}

    [ObservableProperty]
    public partial DateTime? EstimatedPickUpDate {get; set;}

    [ObservableProperty]
    public partial string Description {get; set;} = "";

    /// <summary>Mismas condiciones que ModifyShipmentHandler: envío pendiente y orden sin terminar.</summary>
    public bool CanEdit => Shipment?.Status is ShipmentStatuses.Pending && Order is { IsDone: false };

    public bool CanAdvanceStatus => Shipment?.Status is ShipmentStatuses.Pending or ShipmentStatuses.OnWay && Order is { IsDone: false };

    public bool CanCancel => CanAdvanceStatus;

    public string NextStatusText => Shipment?.Status switch
    {
        ShipmentStatuses.Pending => "Poner en camino",
        ShipmentStatuses.OnWay => "Marcar como entregado",
        _ => ""
    };

    partial void OnShipmentChanged(ShipmentDto? oldValue, ShipmentDto? newValue)
    {
        if(oldValue?.Key != newValue?.Key)
            ResultManager.Clear();

        PackageMenu.Shipment = newValue;
        ResetFields();
    }

    partial void OnOrderChanged(OrderDto? value)
    => PackageMenu.Order = value;

    /// <summary>
    /// Lista de vehículos: los libres más el que ya tiene asignado el envío (que está "En uso" por este mismo envío).
    /// </summary>
    private void UpdateVehicles(bool keepSelection)
    {
        int? selected = keepSelection ? SelectedVehicle?.Id : Shipment?.Vehicle?.Id;

        Vehicles.Clear();
        foreach(var v in _vehicleContext.Vehicles.Where(x => !x.InUse || x.Id == Shipment?.Vehicle?.Id))
            Vehicles.Add(v);

        SelectedVehicle = Vehicles.FirstOrDefault(x => x.Id == selected);
    }

    private void UpdateDrivers(bool keepSelection)
    {
        string? selected = keepSelection ? SelectedDriver?.Code : Shipment?.Driver?.Code;

        Drivers.Clear();
        foreach(var d in _driverContext.Drivers.Where(x => !x.InUse ||
        string.Equals(x.Code, Shipment?.Driver?.Code, StringComparison.OrdinalIgnoreCase)))
            Drivers.Add(d);

        SelectedDriver = Drivers.FirstOrDefault(x => string.Equals(x.Code, selected, StringComparison.OrdinalIgnoreCase));
    }

    [RelayCommand]
    private void ResetFields()
    {
        UpdateVehicles(keepSelection: false);
        UpdateDrivers(keepSelection: false);

        EstimatedPickUpDate = Shipment?.EstimatedPickUpAt?.LocalDateTime.Date;
        Description = Shipment?.Description ?? "";
    }

    [RelayCommand]
    private void ClearVehicle() => SelectedVehicle = null;

    [RelayCommand]
    private void ClearDriver() => SelectedDriver = null;

    [RelayCommand]
    private async Task SaveChanges()
    {
        if(Shipment is null)
            return;

        string code = Shipment.Key;

        // Si la fecha no se tocó se manda la original (con su hora), así no cuenta como "cambio".
        DateTimeOffset? estimated = EstimatedPickUpDate is DateTime date ?
        (Shipment.EstimatedPickUpAt?.LocalDateTime.Date == date.Date ? Shipment.EstimatedPickUpAt : new DateTimeOffset(date.Date)) :
        null;

        try
        {
            var result = await _modifyHandler.Handle(new(code, SelectedDriver?.Code, SelectedVehicle?.Id, estimated, Description));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Envío modificado con éxito");
                    NotifyShipmentChanged(code);
                },
                notFound => ResultManager.MarkError($"No se encontró el envío {code}"),
                vehicleNotFound => ResultManager.MarkError($"No se encontró el vehículo con Id {vehicleNotFound.Id}"),
                driverNotFound => ResultManager.MarkError($"No se encontró el conductor {driverNotFound.Code}"),
                validationError => ResultManager.MarkErrors(validationError.Errors)
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task AdvanceStatus()
    {
        if(Shipment is null)
            return;

        string code = Shipment.Key;

        try
        {
            var result = await _updateHandler.ChangeShipmentState(code);

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Estado del envío actualizado");
                    NotifyShipmentChanged(code);
                },
                notFound => ResultManager.MarkError($"No se encontró el envío {code}"),
                validationError => ResultManager.MarkErrors(validationError.Errors.DefaultIfEmpty
                ("El envío ya no puede cambiar de estado"), "No se pudo cambiar el estado:")
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task CancelShipment()
    {
        if(Shipment is null)
            return;

        string code = Shipment.Key;

        try
        {
            var result = await _updateHandler.CancelateShipment(code);

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Envío cancelado (se liberaron su vehículo y su conductor)");
                    NotifyShipmentChanged(code);
                },
                notFound => ResultManager.MarkError($"No se encontró el envío {code}"),
                validationError => ResultManager.MarkErrors(validationError.Errors.DefaultIfEmpty
                ("El envío ya no se puede cancelar"), "No se pudo cancelar:")
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    /// <summary>Cualquier cambio del envío puede cambiar el "En uso" de vehículos y conductores.</summary>
    private static void NotifyShipmentChanged(string code)
    {
        WeakReferenceMessenger.Default.Send(new ShipmentUpdateMessage([code], Shared.Enums.ContextChangeType.Modification));
        WeakReferenceMessenger.Default.Send(new VehicleUpdateMessage(null, Shared.Enums.ContextChangeType.Other));
        WeakReferenceMessenger.Default.Send(new DriverUpdateMessage(null, Shared.Enums.ContextChangeType.Other));
    }

    [RelayCommand]
    private void Exit()
    {
        PackageMenu.Reset();
        Shipment = null;
        ViewIsClosed?.Invoke();
    }
}
