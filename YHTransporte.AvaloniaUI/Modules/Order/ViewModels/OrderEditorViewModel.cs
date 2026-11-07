using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Order.ViewModels;

public partial class OrderEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial OrderDto? Order {get; set;}
    
    public event Action? ViewIsClosed;

    [RelayCommand]
    public void Close()
    {
        Order = null;

        ViewIsClosed?.Invoke();
    }
}