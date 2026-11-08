using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Deliveries.UseCases.GetDriver;
using YHTransporte.Application.Deliveries.UseCases.ModifyDriver;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;

public partial class DriverEditorViewModel : ViewModelBase, IResultProvider
{
    public DriverEditorViewModel(ModifyDriverHandler modifyHandler, GetDriverHandler getHandler, DriverContext context)
    {
        _modifyHandler = modifyHandler;
        _getHandler = getHandler;
        _context = context;

        _context.DriversChanged += () =>
        {
            if(Driver is not null && _context.GetByCode(Driver.Code) is DriverDto updated)
                Driver = updated;
        };
    }

    private readonly ModifyDriverHandler _modifyHandler;
    private readonly GetDriverHandler _getHandler;
    private readonly DriverContext _context;

    public event Action? ViewIsClosed;

    [ObservableProperty]
    public partial DriverDto? Driver {get; set;}

    [ObservableProperty]
    public partial string Name {get; set;} = "";

    [ObservableProperty]
    public partial string MiddleName {get; set;} = "";

    [ObservableProperty]
    public partial string LastName {get; set;} = "";

    [ObservableProperty]
    public partial string SecondLastName {get; set;} = "";

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    public ObservableCollection<ShipmentHistoryDto> History {get;} = [];

    async partial void OnDriverChanged(DriverDto? oldValue, DriverDto? newValue)
    {
        if(oldValue?.Code != newValue?.Code)
            ResultManager.Clear();

        ResetFields();

        History.Clear();

        if(newValue is null)
            return;

        try
        {
            foreach(var item in await _getHandler.GetDriverHistoryAsync(newValue.Code))
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
        Name = Driver?.Name ?? "";
        MiddleName = Driver?.MiddleName ?? "";
        LastName = Driver?.LastName ?? "";
        SecondLastName = Driver?.SecondLastName ?? "";
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        if(Driver is null)
            return;

        string code = Driver.Code;

        try
        {
            var result = await _modifyHandler.Handle(new(code, Name, MiddleName, LastName, SecondLastName));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Conductor modificado con éxito");
                    WeakReferenceMessenger.Default.Send(new DriverUpdateMessage([code], Shared.Enums.ContextChangeType.Modification));
                },
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
    private void Exit()
    {
        Driver = null;
        ViewIsClosed?.Invoke();
    }
}
