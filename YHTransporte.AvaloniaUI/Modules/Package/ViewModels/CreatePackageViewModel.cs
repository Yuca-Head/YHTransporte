using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.UseCases.AddPackage;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Package.ViewModels;

public partial class CreatePackageViewModel : ViewModelBase, IResultProvider
{
    public CreatePackageViewModel(AddPackageHandler handler, GetThirdPartyHandler thirdPartyHandler)
    {
        _handler = handler;
        _thirdPartyHandler = thirdPartyHandler;
    }

    private readonly AddPackageHandler _handler;
    private readonly GetThirdPartyHandler _thirdPartyHandler;

    [ObservableProperty]
    public partial ShipmentDto? Shipment {get; set;}

    [ObservableProperty]
    public partial OrderDto? Order {get; set;}

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    /// <summary>Productos del proveedor de la orden (un paquete solo puede llevar estos).</summary>
    public ObservableCollection<ProductDto> Products {get;} = [];

    [ObservableProperty]
    public partial ProductDto? SelectedProduct {get; set;}

    [ObservableProperty]
    public partial decimal? Weight {get; set;}

    [ObservableProperty]
    public partial decimal? ProductAmount {get; set;}

    // Se recargan al abrir para tomar productos agregados al proveedor desde el editor de terceros.
    async partial void OnIsOpenChanged(bool value)
    {
        if(value)
            await LoadProducts();
    }

    private async Task LoadProducts()
    {
        Products.Clear();
        SelectedProduct = null;

        if(Order is null)
            return;

        try
        {
            foreach(var product in await _thirdPartyHandler.GetSupplierProducts(Order.Supplier.ThirdPartyKey))
                Products.Add(product);

            if(Products.Count == 0)
                ResultManager.MarkError("El proveedor de esta orden no tiene productos registrados (agréguelos desde Terceros)");
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        if(Shipment is null)
        {
            ResultManager.MarkError("Debe abrir un envío para agregar paquetes");
            return;
        }
        if(SelectedProduct is null)
        {
            ResultManager.MarkError("Debe escoger un producto para el paquete");
            return;
        }
        if(Weight is null || ProductAmount is null)
        {
            ResultManager.MarkError("Debe ingresar el peso y la cantidad del paquete");
            return;
        }
        if(ProductAmount != decimal.Truncate(ProductAmount.Value) || ProductAmount > int.MaxValue)
        {
            ResultManager.MarkError("La cantidad debe ser un número entero");
            return;
        }

        string code = Shipment.Key;

        try
        {
            var result = await _handler.Handle(new(code, SelectedProduct.Key, Weight.Value, (int)ProductAmount.Value));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Paquete agregado con éxito");
                    WeakReferenceMessenger.Default.Send(new ShipmentUpdateMessage([code], Shared.Enums.ContextChangeType.Modification));
                    ClearFields();
                },
                notFound => ResultManager.MarkError($"No se encontró el envío {code}"),
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
        SelectedProduct = null;
        Weight = null;
        ProductAmount = null;
    }

    [RelayCommand]
    public void Close()
    => ResultManager.Clear(() =>
    {
        IsOpen = false;
        ClearFields();
    });
}
