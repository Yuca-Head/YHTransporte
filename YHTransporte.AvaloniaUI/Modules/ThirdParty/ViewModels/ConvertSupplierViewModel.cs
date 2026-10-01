
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class ConvertSupplierViewModel(CreateThirdPartyHandler useCase) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [ObservableProperty]
    public partial string ResultMessage { get; private set; } = "";

    [ObservableProperty]
    public partial bool HasError { get; set; }

    partial void OnIsOpenChanged(bool value)
    {
        if (value)
            return;
        Clear();
    }

    [RelayCommand]
    private void Close()
    {
        IsOpen = false;
        ResultMessage = "";
    }

    [RelayCommand]
    private void Clear()
    {
        HasError = false;
        ResultMessage = "";
    }
}