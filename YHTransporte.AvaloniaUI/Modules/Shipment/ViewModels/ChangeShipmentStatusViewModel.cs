using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Shipment.ViewModels;

public partial class ChangeShipmentStatusViewModel : ViewModelBase
{
    [ObservableProperty]
    private OrderCode? order;


}