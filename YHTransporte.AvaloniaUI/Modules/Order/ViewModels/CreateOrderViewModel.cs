
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Addresses.Dtos;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.Application.Orders.UseCases.CreateOrder;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class CreateOrderViewModel : ViewModelBase
{

    public CreateOrderViewModel(CreateOrderHandler createOrderHandler, ThirdPartyContext tpContext)
    {
        _creatorHandler = createOrderHandler;
        _tpContext = tpContext;

        UpdateParties();

        _tpContext.ThirdPartiesChanged += UpdateParties;
    }
    [ObservableProperty]
    public partial CreateOrderCommand Order{get; set;} = new(){ Creation = DateTime.Now};

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

    [ObservableProperty]
    public partial ThirdPartyDetailsDto? SelectedCustomer{get; set;}
    [ObservableProperty]
    public partial ThirdPartyDetailsDto? SelectedSupplier {get; set;}

    [ObservableProperty]
    public partial AddressDetailsDto Origin {get; set;}
    [ObservableProperty]
    public partial AddressDetailsDto Destination {get; set;}


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

    [RelayCommand]
    public async Task Create()
    {
        
        await _creatorHandler.Handle(Order with 
        {SupplierId = SelectedSupplier.Key, CustomerId = SelectedCustomer.Key, OriginId = Origin.Id, DestinationId = Destination.Id});
    }
}