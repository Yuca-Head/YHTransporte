
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Addresses.Dtos;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.Application.Orders.UseCases.CreateOrder;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class CreateOrderViewModel : ViewModelBase, IResultProvider
{

    public CreateOrderViewModel(CreateOrderHandler createOrderHandler, ThirdPartyContext tpContext)
    {
        _creatorHandler = createOrderHandler;
        _tpContext = tpContext;

        UpdateParties();

        _tpContext.ThirdPartiesChanged += UpdateParties;
    }


    [ObservableProperty]
    public partial CreateOrderCommand Order{get; set;} = new();
    [ObservableProperty]
    public partial ResultManager ResultManager {get; set;} = new();

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

    [ObservableProperty]
    public partial ThirdPartyDetailsDto? SelectedCustomer{get; set;}
    [ObservableProperty]
    public partial ThirdPartyDetailsDto? SelectedSupplier {get; set;}

    [ObservableProperty]
    public partial AddressDetailsDto? Origin {get; set;}
    [ObservableProperty]
    public partial AddressDetailsDto? Destination {get; set;}


    public ObservableCollection<ThirdPartyDetailsDto> Customers {get; init;} = []; 
    public ObservableCollection<ThirdPartyDetailsDto> Suppliers {get; init;} = []; 
    private readonly CreateOrderHandler _creatorHandler;

    private readonly ThirdPartyContext _tpContext;

    private async void UpdateParties()
    {
        Customers.Clear();
        Suppliers.Clear();

        foreach(var tp in _tpContext.ThirdParties)
        {
            if(tp.IsCustomer)
                Customers.Add(tp);

            if(tp.IsSupplier)
                Suppliers.Add(tp);
        }
        
    }

    [RelayCommand]
    private void Close()
    => IsOpen = false;
    
    [RelayCommand]
    private void Open()
    => IsOpen = true;

    [RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = true)]
    public async Task Create(CancellationToken cancellationToken)
    {

        if(!PreOrderQueryValidator())
            return;
        var result = await _creatorHandler.Handle(Order with 
        {SupplierId = SelectedSupplier!.Key, CustomerId = SelectedCustomer!.Key
        , OriginId = Origin!.Id, DestinationId = Destination!.Id},
        cancellationToken);
        
        result.Switch(
            success =>
            {
                ResultManager.Success("Orden creada con éxito");
                WeakReferenceMessenger.Default.Send<OrderUpdateMessage>(new([], Shared.Enums.ContextChangeType.Creation));
            },
            addressNotfound => ResultManager.MarkError($"Dirección: '{addressNotfound.Ids.FirstOrDefault()}' no econtrada"),

            customerNotFound => ResultManager.MarkError($"Cliente no encontrado: {customerNotFound.Key} (id)"),
            supplierNotFound => ResultManager.MarkError($"Proveedor no encontrado: {supplierNotFound.Key} (id)"),
            notFound => ResultManager.MarkError(notFound.Argument)
        );

        
    }

    private bool PreOrderQueryValidator()
    {
        if(SelectedCustomer is null)
        {
            ResultManager.MarkError("Debe escoger un cliente para la orden");
            return false;
        }
        if(SelectedSupplier is null)
        {
            ResultManager.MarkError("Debe escoger un proveedor para la orden");
            return false;
        }
        if(Origin is null)
        {
            ResultManager.MarkError("Debe escoger un origen para la orden");
            return false;
        }
        if(Destination is null)
        {
            ResultManager.MarkError("Debe escoger un destino para la orden");
            return false;
        }

        return true;
    }


}