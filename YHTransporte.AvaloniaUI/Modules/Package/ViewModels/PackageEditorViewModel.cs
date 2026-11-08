using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.Shipments.UseCases.ModifyPackage;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Modules.Package.ViewModels;

/// <summary>
/// Editor mínimo de un paquete (producto, peso y cantidad).
/// Solo se puede modificar con las mismas condiciones que para crearlo.
/// </summary>
public partial class PackageEditorViewModel : ViewModelBase, IResultProvider
{
    public PackageEditorViewModel(ModifyPackageHandler handler, GetThirdPartyHandler thirdPartyHandler)
    {
        _handler = handler;
        _thirdPartyHandler = thirdPartyHandler;
    }

    private readonly ModifyPackageHandler _handler;
    private readonly GetThirdPartyHandler _thirdPartyHandler;

    public event Action? ViewIsClosed;

    [ObservableProperty]
    public partial PackageDto? Package {get; set;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    public partial ShipmentDto? Shipment {get; set;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    public partial OrderDto? Order {get; set;}

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    public ObservableCollection<ProductDto> Products {get;} = [];

    [ObservableProperty]
    public partial ProductDto? SelectedProduct {get; set;}

    [ObservableProperty]
    public partial decimal? Weight {get; set;}

    [ObservableProperty]
    public partial decimal? ProductAmount {get; set;}

    public bool CanEdit => Shipment?.Status is ShipmentStatuses.Pending && Order is { IsDone: false };

    async partial void OnPackageChanged(PackageDto? oldValue, PackageDto? newValue)
    {
        if(oldValue?.Key != newValue?.Key)
            ResultManager.Clear();

        if(newValue is null)
            return;

        await LoadProducts();
        ResetFields();
    }

    private async Task LoadProducts()
    {
        Products.Clear();

        try
        {
            if(Order is not null)
                foreach(var product in await _thirdPartyHandler.GetSupplierProducts(Order.Supplier.ThirdPartyKey))
                    Products.Add(product);
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }

        // Si el producto ya no es del proveedor igual se muestra, para que se vea cuál tenía.
        if(Package is not null && !Products.Any(x => x.Key == Package.Product.Key))
            Products.Add(Package.Product);
    }

    [RelayCommand]
    private void ResetFields()
    {
        SelectedProduct = Package is null ? null : Products.FirstOrDefault(x => x.Key == Package.Product.Key);
        Weight = Package?.Weight;
        ProductAmount = Package?.ProductAmount;
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        if(Package is null || Shipment is null)
            return;

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
            var result = await _handler.Handle(new(code, Package.Key, SelectedProduct.Key, Weight.Value, (int)ProductAmount.Value));

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Paquete modificado con éxito");
                    WeakReferenceMessenger.Default.Send(new ShipmentUpdateMessage([code], Shared.Enums.ContextChangeType.Modification));
                },
                notFound => ResultManager.MarkError($"No se encontró el paquete {Package.Key} del envío {code}"),
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
        Package = null;
        ViewIsClosed?.Invoke();
    }
}
