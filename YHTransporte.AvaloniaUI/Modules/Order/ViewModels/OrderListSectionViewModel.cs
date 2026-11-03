using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.Application.Orders.UseCases.CreateOrder;
using YHTransporte.AvaloniaUI.Modules.Order.Models;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class OrderListSectionViewModel : ViewModelBase
{

    public OrderListSectionViewModel(CreateOrderViewModel ordercreator, OrderContext context, 
    ThirdPartyContext thirdPartyContext)
    {
        _context = context;
        _thirdPartyContext = thirdPartyContext;
        Creator = ordercreator;
        UpdateMeAsync();
        
        _context.OrdersChanged += UpdateMeAsync;
    }
    public event Action? SelectedOrderChanged;
    
    
    [ObservableProperty]
    public partial CreateOrderViewModel Creator {get; private set;}

    [ObservableProperty]
    public partial CreateOrderCommand? SelectedOrder {get; set;}
    

    public ObservableCollection<OrderDetailsVM> OrderList {get;} = [];

    private readonly OrderContext _context;
    private readonly ThirdPartyContext _thirdPartyContext;

    public void SelectedOrderHasChanged()
    {
        SelectedOrderChanged?.Invoke();
    }
    
    private async void UpdateMeAsync()
    {
        OrderList.Clear();

        foreach(var order in _context.Orders)
            OrderList.Add(await OrderDetailsVM.CreateFromContext(order, _thirdPartyContext));
    }



    // Un mensaje para todos los aliens del mundo:
    //zi5PJeMszMWD.. ..-. /DrA0470tHhtRlTa7C1olXMOdG6N4mxZQgMyI558C9mWctxYKzP29pY8jE5GimVe7URPFqekjlFaPCuaRhe6IWIEDkx7YHeqRZ3KBEP8HYieaxANNMxk4X9wnXhFGyE3cD8brL-.-- --- ..- .----. .-. . /ExSySQLrcPNeum0N0ultraMicr0scoPicSilic0vucANOconi0sISWC4666J7cS9+505U80JlU5fVTjU22UELDS8m3pHoilSb8jPiJYgFDA5VOoQvjPADIeHKtz5Dll2OnT5VFvA3f8wUWiNKfMWFRGgmeQErYcEgUAN5x2F5wwjujzq4rxloSMBt4didUfindMe?Dd7Rfmd2fG2aez6UA9sSCzP8M8Urz72qxJ6q3hsVud7..... ..... ..... --..-- /G1uvqNfxa3DJnvZiJsFc72UqoYBF4CnS6nKmmX2QHlo9I0vfgdYw8OkUuS9gyy .... . -. /RKpRyGkorQwJfVGa2h48wDdPwN7sxF8jy57dw8ioZHFbyrumcEivt9DfVEKJ6RynULLtSNMUnMifLBAJfjRr06lQQ0YH3Bo4k7mjBroZvp5yMwF3zfYykwq8rneVHp4Lj7LpXwV8m.. .----. -- /ryNru4t7LVypiU40rOj5FrRvW6NTLM0MdIxOyRef02IEnKkBaQ0UvszlfYB1GslOxTBnlDCDALoGUNvpOyICs2LB8Jr7cTKAOXKrc7AYcuURTA1kUXqA1FcM28O4i5lVZKzm0ITyHs7BUolP5lW3v29MrSG9EvLAasPwzctfK7Nvoch0l4AShouldIproccedOMeX8B8mCpeqeAkAzOweN33dt0Cukce1ourqMLQylHpPPtvpK2GxWo4fk7zZGFwC1VO1RENaVp6CmwsTDccJP5C...orturnBACK'roundnsybyUQWvaLE7rz87cDAOYDCSZFStLC3h36e-L0RDOS3HGA7c3MC8yeM3dfa7.LETMEknowrKrgvkCbqxczITfL6BkDjOkL.WHATishouldzufIzQpMRf0pFd500DO500At1UcpdCUqP7PkqUNiQ19xoivT3zMbPIzkaGV6WKExRq-.... -.... -....1D-SOdidIpassedTh3class¿RqGlrOQJaO1TaB6ng2j0e5Qm6X7R7CtKq2otuP5hFjl5otZs3O72SGM7pHUZwIfUo9TxsBZIyTzvaRMNq2
    
}