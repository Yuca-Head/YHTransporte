using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.Models;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Entities;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class CreateThirdPartyViewModel : ViewModelBase
{

    public CreateThirdPartyViewModel(CreateThirdPartyHandler useCase)
    {
        _useCase = useCase;
    }

    
    [ObservableProperty]
    public partial Queue<string> ErrorMessages {get; set;} = [];

    [ObservableProperty]
    public partial CreateThirdPartyModel NewThirdParty{ get; set; } = new("");
    private readonly CreateThirdPartyHandler _useCase;
    public event EventHandler? CustomerCreated;

    [ObservableProperty]
    public partial string ResultMessage {get; private set;} = "";
    
    [ObservableProperty]
    public partial bool HasError {get; set;} 

    [ObservableProperty]
    public partial bool IsOpen {get; set;}


    partial void OnIsOpenChanged(bool value)
    {
        if(value)
            return;
        Clear();
    }

    [RelayCommand]
    private async Task CreateCustomer()
    {
        var result = await _useCase.Handle(new CreateThirdPartyCommand(NewThirdParty.Name, true));

        result.Switch
        (
            success => 
            {
                ResultMessage = "Cliente Creado con éxito";
                CustomerCreated?.Invoke(HasError, EventArgs.Empty);
                WeakReferenceMessenger.Default.Send<ThirdPartyUpdateMessage>(new(null, Shared.Enums.ContextChangeType.Creation));
                Clear();
            },

            alreadyExists => MarkErrors(alreadyExists, (m) =>
            {
                if(m.Count() == 1)
                    return $"Ya existe un tercero con ese nombre {m.First()}";

                var sb = new StringBuilder();
                
                sb.Append("Ya existen los siguientes terceros ingresados: ");
                foreach(var msg in m)
                    sb.Append($"{msg}, ");
                
                return sb.ToString();
            }
            ),

            validationError => MarkErrors(validationError.Errors, (m) =>
            {
                var sb = new StringBuilder();

                foreach(var msg in m)
                    sb.Append(msg);
                
                return sb.ToString();
            }
            ),
            repeatedValue => MarkErrors(repeatedValue, (m) => $"Se ingresaron dos clientes de mismo nombre {m}")
        );
    }
    
    private void MarkErrors(object? arg, Func<IEnumerable<string>, string> message)
    {
        if(ResultHandler.TryParseToString(arg, out var results))
            ResultMessage = message.Invoke(results);
                
        HasError = results.Any();
    }

    [RelayCommand]
    private void Close()
    {
        IsOpen = false;
        ResultMessage = "";
    }

    private void Clear()
    {
        ErrorMessages.Clear();
        HasError = false;
    }
    
}