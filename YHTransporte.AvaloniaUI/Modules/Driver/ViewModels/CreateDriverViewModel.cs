using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Deliveries.UseCases.CreateDriver;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;

public partial class CreateDriverViewModel(CreateDriverHandler handler) : ViewModelBase, IResultProvider
{
    private readonly CreateDriverHandler _handler = handler;

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

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

    [RelayCommand]
    private async Task Create()
    {
        try
        {
            var result = await _handler.Handle(new(Name, MiddleName, LastName, SecondLastName));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Conductor registrado con éxito (el código se generó automáticamente)");
                    WeakReferenceMessenger.Default.Send(new DriverUpdateMessage(null, Shared.Enums.ContextChangeType.Creation));
                    ClearFields();
                },
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
        Name = "";
        MiddleName = "";
        LastName = "";
        SecondLastName = "";
    }

    [RelayCommand]
    private void Close()
    => ResultManager.Clear(() =>
    {
        IsOpen = false;
        ClearFields();
    });
}
