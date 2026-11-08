using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.UseCases.CreateSupplierProduct;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.Application.ThirdParties.UseCases.ModifySupplierProduct;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.Extra;

/// <summary>
/// Productos de un proveedor dentro del editor de terceros (igual que ThirdPartyAddressCreator con las direcciones):
/// lista, agregar uno nuevo y modificar el seleccionado.
/// </summary>
public partial class ThirdPartyProductManager : ObservableObject, IResultProvider
{
    public ThirdPartyProductManager(GetThirdPartyHandler getHandler, CreateSupplierProductHandler createHandler,
    ModifySupplierProductHandler modifyHandler)
    {
        _getHandler = getHandler;
        _createHandler = createHandler;
        _modifyHandler = modifyHandler;
    }

    private readonly GetThirdPartyHandler _getHandler;
    private readonly CreateSupplierProductHandler _createHandler;
    private readonly ModifySupplierProductHandler _modifyHandler;

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSupplier))]
    public partial ThirdPartyDetailsDto? ThirdParty {get; set;}

    public bool IsSupplier => ThirdParty?.IsSupplier ?? false;

    public ObservableCollection<ProductDto> Products {get;} = [];

    // ── Nuevo producto ──
    [ObservableProperty]
    public partial string NewCode {get; set;} = "";

    [ObservableProperty]
    public partial string NewName {get; set;} = "";

    // ── Producto seleccionado (edición) ──
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    public partial ProductDto? SelectedProduct {get; set;}

    [ObservableProperty]
    public partial string EditCode {get; set;} = "";

    [ObservableProperty]
    public partial string EditName {get; set;} = "";

    public bool IsEditing => SelectedProduct is not null;

    async partial void OnThirdPartyChanged(ThirdPartyDetailsDto? oldValue, ThirdPartyDetailsDto? newValue)
    {
        if(oldValue?.Key != newValue?.Key)
        {
            ResultManager.Clear();
            NewCode = "";
            NewName = "";
        }

        await LoadProducts();
    }

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        EditCode = value?.Code ?? "";
        EditName = value?.Name ?? "";
    }

    private async Task LoadProducts(int? selectKey = null)
    {
        SelectedProduct = null;
        Products.Clear();

        if(ThirdParty is null || !ThirdParty.IsSupplier)
            return;

        try
        {
            foreach(var product in await _getHandler.GetSupplierProducts(ThirdParty.Key))
                Products.Add(product);

            if(selectKey is int key)
                SelectedProduct = Products.FirstOrDefault(x => x.Key == key);
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        if(ThirdParty is null)
        {
            ResultManager.MarkError("Debe abrir un proveedor para agregarle productos");
            return;
        }

        try
        {
            var result = await _createHandler.Handle(new(ThirdParty.Key, NewCode, NewName));

            await result.Match
            (
                async success =>
                {
                    ResultManager.Success($"Producto {success.Value.Code} agregado al proveedor");
                    NewCode = "";
                    NewName = "";
                    await LoadProducts();
                },
                supplierNotFound =>
                {
                    ResultManager.MarkError($"El tercero {supplierNotFound.Key} no existe o no es proveedor");
                    return Task.CompletedTask;
                },
                validationError =>
                {
                    ResultManager.MarkErrors(validationError.Errors);
                    return Task.CompletedTask;
                },
                alreadyContains =>
                {
                    ResultManager.MarkError($"Este proveedor ya tiene el producto con código {alreadyContains.Repeated}");
                    return Task.CompletedTask;
                },
                alreadyExists =>
                {
                    ResultManager.MarkError($"El código {alreadyExists.Argument.Code} ya pertenece al producto " +
                    $"\"{alreadyExists.Argument.Name}\". Use el mismo nombre para asignarlo a este proveedor o escoja otro código.");
                    return Task.CompletedTask;
                }
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        if(ThirdParty is null || SelectedProduct is null)
        {
            ResultManager.MarkError("Debe seleccionar un producto para modificarlo");
            return;
        }

        int productKey = SelectedProduct.Key;

        try
        {
            var result = await _modifyHandler.Handle(new(ThirdParty.Key, productKey, EditCode, EditName));

            await result.Match
            (
                async success =>
                {
                    ResultManager.Success("Producto modificado (el cambio aplica a todos los proveedores que lo vendan)");
                    await LoadProducts(productKey);
                },
                supplierNotFound =>
                {
                    ResultManager.MarkError($"El tercero {supplierNotFound.Key} no existe o no es proveedor");
                    return Task.CompletedTask;
                },
                notFound =>
                {
                    ResultManager.MarkError($"El producto {notFound.Argument} no pertenece a este proveedor");
                    return Task.CompletedTask;
                },
                validationError =>
                {
                    ResultManager.MarkErrors(validationError.Errors);
                    return Task.CompletedTask;
                },
                alreadyExists =>
                {
                    ResultManager.MarkError($"El código {alreadyExists.Argument.Code} ya pertenece al producto \"{alreadyExists.Argument.Name}\"");
                    return Task.CompletedTask;
                }
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private void CancelEdit()
    => SelectedProduct = null;

    public void Clear()
    => ResultManager.Clear(() =>
    {
        ThirdParty = null;
        NewCode = "";
        NewName = "";
        SelectedProduct = null;
    });
}
