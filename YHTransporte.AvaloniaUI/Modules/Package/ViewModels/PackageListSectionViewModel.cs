using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Orders.Dtos;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.ViewModels;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Modules.Package.ViewModels;

public partial class PackageListSectionViewModel : ViewModelBase
{
    public PackageListSectionViewModel(CreatePackageViewModel creator)
    {
        Creator = creator;
    }

    public event Action? SelectedPackageChanged;

    public CreatePackageViewModel Creator {get;}

    public ObservableCollection<PackageDto> Packages {get;} = [];

    [ObservableProperty]
    public partial PackageDto? SelectedPackage {get; set;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddPackages))]
    public partial ShipmentDto? Shipment {get; set;}

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddPackages))]
    public partial OrderDto? Order {get; set;}

    /// <summary>Solo si el envío aún no ha partido y la orden no está terminada (regla de AddPackageHandler).</summary>
    public bool CanAddPackages => Shipment?.Status is ShipmentStatuses.Pending && Order is { IsDone: false };

    partial void OnShipmentChanged(ShipmentDto? value)
    {
        Creator.Shipment = value;

        Packages.Clear();

        if(value is null)
            return;

        foreach(var package in value.Packages.OrderBy(x => x.Key))
            Packages.Add(package);
    }

    partial void OnOrderChanged(OrderDto? value)
    => Creator.Order = value;

    public void SelectedPackageHasChanged()
    => SelectedPackageChanged?.Invoke();

    [RelayCommand]
    public void OpenCreator()
    => Creator.IsOpen = true;
}
