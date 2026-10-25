using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class OrderEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial OrderCode? Order {get; set;}
    
}