using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Orders.UseCases.UpdateOrder;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

/// <summary>
/// Editor de una orden: información general, cambio de estado (en espera -> en tránsito -> terminada),
/// cancelación y, abajo, el menú de envíos de la orden (<see cref="ShipmentMenuViewModel"/>).
/// </summary>
public partial class OrderEditorViewModel : ViewModelBase, IResultProvider
{
    public OrderEditorViewModel(UpdateOrderHandler updateHandler, OrderContext orderContext, ThirdPartyContext thirdPartyContext,
    ShipmentContext shipmentContext, ShipmentMenuViewModel shipmentMenu)
    {
        _updateHandler = updateHandler;
        _orderContext = orderContext;
        _thirdPartyContext = thirdPartyContext;
        _shipmentContext = shipmentContext;
        ShipmentMenu = shipmentMenu;

        // Si la orden cambia en el contexto (estado, cancelación) se vuelve a leer.
        _orderContext.OrdersChanged += () =>
        {
            if(Order is not null && _orderContext.GetByCode(Order.Code) is OrderDto updated)
                Order = updated;
        };

        _shipmentContext.ShipmentsChanged += UpdateSummary;
    }

    private readonly UpdateOrderHandler _updateHandler;
    private readonly OrderContext _orderContext;
    private readonly ThirdPartyContext _thirdPartyContext;
    private readonly ShipmentContext _shipmentContext;

    public event Action? ViewIsClosed;

    public ShipmentMenuViewModel ShipmentMenu {get;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdvanceStatus), nameof(CanCancel), nameof(NextStatusText))]
    public partial OrderDto? Order {get; set;}

    [ObservableProperty]
    public partial ThirdPartyDto? Customer {get; private set;}

    [ObservableProperty]
    public partial ThirdPartyDto? Supplier {get; private set;}

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    /// <summary>Se muestra cuando la cancelación normal falló porque hay envíos en camino.</summary>
    [ObservableProperty]
    public partial bool CanForceCancel {get; private set;}

    // ── Resumen de envíos ──
    [ObservableProperty]
    public partial int TotalShipments {get; private set;}

    [ObservableProperty]
    public partial int ActiveShipments {get; private set;}

    [ObservableProperty]
    public partial int DeliveredShipments {get; private set;}

    [ObservableProperty]
    public partial int TotalPackages {get; private set;}

    [ObservableProperty]
    public partial decimal TotalWeight {get; private set;}

    public bool CanAdvanceStatus => Order is { IsDone: false };
    public bool CanCancel => Order is { IsDone: false };

    public string NextStatusText => Order?.Status switch
    {
        OrderStatuses.Pending => "Pasar a tránsito",
        OrderStatuses.InProgress => "Terminar orden",
        _ => ""
    };

    async partial void OnOrderChanged(OrderDto? oldValue, OrderDto? newValue)
    {
        ShipmentMenu.Order = newValue;

        // Solo se limpia el mensaje cuando se abre OTRA orden (no cuando se refresca la misma).
        if(oldValue?.Code != newValue?.Code)
        {
            ResultManager.Clear();
            CanForceCancel = false;
        }

        UpdateSummary();

        if(newValue is null)
        {
            Customer = null;
            Supplier = null;
            return;
        }

        Customer = await _thirdPartyContext.GetById(newValue.Customer.ThirdPartyKey);
        Supplier = await _thirdPartyContext.GetById(newValue.Supplier.ThirdPartyKey);
    }

    private void UpdateSummary()
    {
        IReadOnlyCollection<ShipmentDto> shipments = Order is null ? [] : _shipmentContext.GetByOrder(Order.Code);

        TotalShipments = shipments.Count;
        ActiveShipments = shipments.Count(x => x.Status is ShipmentStatuses.Pending or ShipmentStatuses.OnWay);
        DeliveredShipments = shipments.Count(x => x.Status is ShipmentStatuses.Delivered);
        TotalPackages = shipments.Where(x => x.Status is not ShipmentStatuses.Canceled).Sum(x => x.PackageCount);
        TotalWeight = shipments.Where(x => x.Status is not ShipmentStatuses.Canceled).Sum(x => x.TotalWeight);
    }

    [RelayCommand]
    private async Task AdvanceStatus()
    {
        if(Order is null)
            return;

        string code = Order.Code;

        try
        {
            var result = await _updateHandler.UpdateOrderStatus(code);

            result.Switch
            (
                success =>
                {
                    ResultManager.Success("Estado de la orden actualizado");
                    NotifyOrderChanged(code);
                },
                notFound => ResultManager.MarkError($"No se encontró la orden {code}"),
                shipmentsUncompleted => ResultManager.MarkErrors(shipmentsUncompleted.ShipmentsCode,
                "No se puede terminar la orden, estos envíos aún no se entregan ni cancelan:"),
                validationError => ResultManager.MarkErrors(validationError.Errors.DefaultIfEmpty
                ("La orden ya no puede cambiar de estado"))
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if(Order is null)
            return;

        string code = Order.Code;

        try
        {
            var result = await _updateHandler.CancelateOrder(code);

            result.Switch
            (
                success =>
                {
                    CanForceCancel = false;
                    ResultManager.Success("Orden cancelada (también sus envíos pendientes)");
                    NotifyOrderChanged(code, shipmentsChanged: true);
                },
                notFound => ResultManager.MarkError($"No se encontró la orden {code}"),
                shipmentsUncompleted =>
                {
                    CanForceCancel = true;
                    ResultManager.MarkErrors(shipmentsUncompleted.ShipmentsCode,
                    "No se canceló nada: estos envíos van en camino (use \"Forzar cancelación\" para cancelarlos también):");
                },
                validationError => ResultManager.MarkErrors(validationError.Errors.DefaultIfEmpty
                ("La orden ya está terminada o cancelada"))
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    [RelayCommand]
    private async Task ForceCancel()
    {
        if(Order is null)
            return;

        string code = Order.Code;

        try
        {
            var result = await _updateHandler.ForceCancelateOrder(code);

            result.Switch
            (
                success =>
                {
                    CanForceCancel = false;
                    ResultManager.Success("Orden cancelada junto con todos sus envíos no entregados");
                    NotifyOrderChanged(code, shipmentsChanged: true);
                },
                notFound => ResultManager.MarkError($"No se encontró la orden {code}"),
                validationError => ResultManager.MarkErrors(validationError.Errors.DefaultIfEmpty
                ("La orden ya está terminada o cancelada"))
            );
        }
        catch(Exception ex)
        {
            ResultManager.MarkUnexpected(ex);
        }
    }

    /// <summary>Avisa a los contextos; al cancelar también cambian envíos, vehículos y conductores (se liberan).</summary>
    private static void NotifyOrderChanged(string code, bool shipmentsChanged = false)
    {
        WeakReferenceMessenger.Default.Send(new OrderUpdateMessage([code], Shared.Enums.ContextChangeType.Modification));

        if(!shipmentsChanged)
            return;

        WeakReferenceMessenger.Default.Send(new ShipmentUpdateMessage(null, Shared.Enums.ContextChangeType.Modification));
        WeakReferenceMessenger.Default.Send(new VehicleUpdateMessage(null, Shared.Enums.ContextChangeType.Other));
        WeakReferenceMessenger.Default.Send(new DriverUpdateMessage(null, Shared.Enums.ContextChangeType.Other));
    }

    [RelayCommand]
    public void Close()
    {
        ShipmentMenu.Reset();
        Order = null;

        ViewIsClosed?.Invoke();
    }
}
