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
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Entities;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class CreateThirdPartyViewModel : ViewModelBase
{

    public CreateThirdPartyViewModel(CreateThirdPartyHandler useCase, AddressContext addressContext)
    {
        _useCase = useCase;
        _addressContext = addressContext;
    }

    
    [ObservableProperty]
    public partial Queue<string> ErrorMessages {get; set;} = [];

    private readonly AddressContext _addressContext;



    [ObservableProperty]
    public partial CreateThirdPartyCommand NewThirdParty{ get; set; } = new();
    private readonly CreateThirdPartyHandler _useCase;

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
    private async Task Create()
    {
        if(!NewThirdParty.IsSupplier && !NewThirdParty.IsCustomer)
        {
            ResultMessage = "Para crear un tercero se le debe asignar al menos un rol";
            HasError = true;
            return;
        }
        var result = await _useCase.Handle(NewThirdParty);

        result.Switch
        (
            success => 
            {
                ResultMessage = "Tercero Creado con éxito";
    
                WeakReferenceMessenger.Default.Send<ThirdPartyUpdateMessage>(new(null, Shared.Enums.ContextChangeType.Creation));
                Clear();
            },

            alreadyExists => 
            {
                var args = alreadyExists.Argument.ToArray();
                if(args.Length == 1)
                {
                    ResultMessage = $"Ya existe un tercero con ese nombre ({args.First()})";
                    return;
                }
                var sb = new StringBuilder();
                
                sb.Append("Ya existen los siguientes terceros ingresados: ");
                foreach(var msg in args)
                    sb.Append($"{msg}, ");
                
                ResultMessage = sb.ToString();
            },

            validationError =>
            {
                var args = validationError.Errors;
                var sb = new StringBuilder("Se han identificado los siguientes errores: ");

                foreach(var msg in args)
                    sb.Append(msg);
                
                ResultMessage = sb.ToString();
            },
            repeatedValue => ResultMessage = $"Se ingresaron dos clientes de mismo nombre {repeatedValue.Argument.FirstOrDefault()}"
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
        NewThirdParty = new();
    }
    
}