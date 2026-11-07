using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using YHTransporte.Application.Orders.Extra;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Dashboard.ViewModels;

public partial class DashboardListSectionViewModel : ViewModelBase
{
    [ObservableProperty]
    private OrderCode? order;

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [RelayCommand]
    private void Close() => IsOpen = false;


}