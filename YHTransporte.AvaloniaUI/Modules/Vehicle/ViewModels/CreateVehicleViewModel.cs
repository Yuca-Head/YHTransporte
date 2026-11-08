using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Deliveries.UseCases.CreateVehicle;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Vehicle.ViewModels;

public partial class CreateVehicleViewModel(CreateVehicleHandler handler) : ViewModelBase, IResultProvider
{
    private readonly CreateVehicleHandler _handler = handler;

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [ObservableProperty]
    public partial string Plate {get; set;} = "";

    [ObservableProperty]
    public partial string Description {get; set;} = "";

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    [RelayCommand]
    private async Task Create()
    {
        try
        {
            var result = await _handler.Handle(new(Plate, Description));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Vehículo registrado con éxito");
                    WeakReferenceMessenger.Default.Send(new VehicleUpdateMessage(null, Shared.Enums.ContextChangeType.Creation));
                    Plate = "";
                    Description = "";
                },
                validationError => ResultManager.MarkErrors(validationError.Errors),
                alreadyExists => ResultManager.MarkError($"Ya existe un vehículo con la placa {alreadyExists.Argument}")
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private void Close()
    => ResultManager.Clear(() =>
    {
        IsOpen = false;
        Plate = "";
        Description = "";
    });
}
