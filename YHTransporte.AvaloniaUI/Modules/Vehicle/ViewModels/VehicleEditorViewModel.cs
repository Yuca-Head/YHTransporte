using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Deliveries.UseCases.GetVehicle;
using YHTransporte.Application.Deliveries.UseCases.ModifyVehicle;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class VehicleEditorViewModel : ViewModelBase, IResultProvider
{
    public VehicleEditorViewModel(ModifyVehicleHandler modifyHandler, GetVehicleHandler getHandler, VehicleContext context)
    {
        _modifyHandler = modifyHandler;
        _getHandler = getHandler;
        _context = context;

        // Si cambia en el contexto (por ejemplo, pasa a "En uso" por un envío) se vuelve a leer.
        _context.VehiclesChanged += () =>
        {
            if(Vehicle is not null && _context.GetById(Vehicle.Id) is VehicleDto updated)
                Vehicle = updated;
        };
    }

    private readonly ModifyVehicleHandler _modifyHandler;
    private readonly GetVehicleHandler _getHandler;
    private readonly VehicleContext _context;

    public event Action? ViewIsClosed;

    // CAMBIO: antes era OrderCode? (marcador de posición); ahora es el vehículo real.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditPlate))]
    public partial VehicleDto? Vehicle {get; set;}

    [ObservableProperty]
    public partial string Plate {get; set;} = "";

    [ObservableProperty]
    public partial string Description {get; set;} = "";

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    public ObservableCollection<ShipmentHistoryDto> History {get;} = [];

    /// <summary>La placa no se cambia mientras el vehículo está en un envío.</summary>
    public bool CanEditPlate => Vehicle is { InUse: false };

    async partial void OnVehicleChanged(VehicleDto? oldValue, VehicleDto? newValue)
    {
        if(oldValue?.Id != newValue?.Id)
            ResultManager.Clear();

        ResetFields();

        History.Clear();

        if(newValue is null)
            return;

        try
        {
            foreach(var item in await _getHandler.GetVehicleHistoryAsync(newValue.Id))
                History.Add(item);
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private void ResetFields()
    {
        Plate = Vehicle?.Plate ?? "";
        Description = Vehicle?.Description ?? "";
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        if(Vehicle is null)
            return;

        int id = Vehicle.Id;

        try
        {
            var result = await _modifyHandler.Handle(new(id, Plate, Description));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Vehículo modificado con éxito");
                    WeakReferenceMessenger.Default.Send(new VehicleUpdateMessage([id], Shared.Enums.ContextChangeType.Modification));
                },
                vehicleNotFound => ResultManager.MarkError($"No se encontró el vehículo con Id {vehicleNotFound.Id}"),
                validationError => ResultManager.MarkErrors(validationError.Errors),
                alreadyExists => ResultManager.MarkError($"Ya existe otro vehículo con la placa {alreadyExists.Argument}")
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Vehicle = null;
        ViewIsClosed?.Invoke();
    }

}
