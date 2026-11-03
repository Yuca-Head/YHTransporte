using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Tmds.DBus.Protocol;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Orders.UseCases.GetOrder;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;

public sealed class OrderContext : IInitiableContext
{
    public OrderContext(GetOrderHandler orderHandler)
    {
        _handler = orderHandler;
        
        WeakReferenceMessenger.Default.Register<OrderUpdateMessage>(this, async (_, m) =>
        {

            if(m.ChangeType is Enums.ContextChangeType.Creation)
            {
                await ReDoOrders();
                OrdersChanged?.Invoke();
                return;
            }
        
            await GenericContexts.UpdateDtoListByOne(_orders, m, 
            async (x, token) => 
            {
                var result = await _handler.GetByCode(new(x), token);


                if(!result.IsT1)
                    throw new InvalidOperationException("Every order key was expected to exist");
                return result.AsT0.Value;

            }, o => o.Code); //O comes from "o-rder"

            OrdersChanged?.Invoke();
        });
    }

    public event Action? OrdersChanged;

    private readonly GetOrderHandler _handler;
    private readonly Dictionary<string, OrderDto> _orders = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<OrderDto> Orders => _orders.Values;
    
    public async Task Init()
    {
        await ReDoOrders();
    }

    private async Task ReDoOrders()
    {
        _orders.Clear();

        foreach(var order in await _handler.GetEverythingAsync())
            _orders.Add(order.Code, order);
    }

}