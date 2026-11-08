using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.UseCases.CreateShipment;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;

public partial class CreateShipmentViewModel : ViewModelBase, IResultProvider
{
    public CreateShipmentViewModel(CreateShipmentHandler handler, VehicleContext vehicleContext, DriverContext driverContext)
    {
        _handler = handler;
        _vehicleContext = vehicleContext;
        _driverContext = driverContext;

        _vehicleContext.VehiclesChanged += UpdateVehicles;
        _driverContext.DriversChanged += UpdateDrivers;

        UpdateVehicles();
        UpdateDrivers();
    }

    private readonly CreateShipmentHandler _handler;
    private readonly VehicleContext _vehicleContext;
    private readonly DriverContext _driverContext;

    [ObservableProperty]
    public partial OrderDto? Order {get; set;}

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    /// <summary>Solo vehículos y conductores libres (no "En uso").</summary>
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

    private void UpdateVehicles()
    {
        int? selected = SelectedVehicle?.Id;

        Vehicles.Clear();
        foreach(var v in _vehicleContext.AvailableVehicles)
            Vehicles.Add(v);

        SelectedVehicle = selected is int id && _vehicleContext.GetById(id) is { InUse: false } v2 ? v2 : null;
    }

    private void UpdateDrivers()
    {
        string? selected = SelectedDriver?.Code;

        Drivers.Clear();
        foreach(var d in _driverContext.AvailableDrivers)
            Drivers.Add(d);

        SelectedDriver = selected is not null && _driverContext.GetByCode(selected) is { InUse: false } d2 ? d2 : null;
    }

    [RelayCommand]
    private void ClearVehicle() => SelectedVehicle = null;

    [RelayCommand]
    private void ClearDriver() => SelectedDriver = null;

    [RelayCommand]
    private async Task Create()
    {
        if(Order is null)
        {
            ResultManager.MarkError("Debe abrir una orden para registrar un envío");
            return;
        }

        DateTimeOffset? estimated = EstimatedPickUpDate is DateTime date ? new DateTimeOffset(date.Date) : null;

        try
        {
            var result = await _handler.Handle(new(Order.Code, SelectedDriver?.Code, SelectedVehicle?.Id, estimated, Description));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Envío registrado con éxito");

                    // El envío nuevo ocupa al vehículo y al conductor (InUse = 1).
                    WeakReferenceMessenger.Default.Send(new ShipmentUpdateMessage(null, Shared.Enums.ContextChangeType.Creation));
                    WeakReferenceMessenger.Default.Send(new VehicleUpdateMessage(null, Shared.Enums.ContextChangeType.Other));
                    WeakReferenceMessenger.Default.Send(new DriverUpdateMessage(null, Shared.Enums.ContextChangeType.Other));

                    ClearFields();
                },
                notFound => ResultManager.MarkError($"No se encontró la orden {Order.Code}"),
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

    private void ClearFields()
    {
        SelectedVehicle = null;
        SelectedDriver = null;
        EstimatedPickUpDate = null;
        Description = "";
    }

    [RelayCommand]
    public void Close()
    => ResultManager.Clear(() =>
    {
        IsOpen = false;
        ClearFields();
    });
}
