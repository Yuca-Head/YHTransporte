using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class ThirdPartyEditorMenuViewModel : ViewModelBase
{

    public ThirdPartyEditorMenuViewModel(CreateThirdPartyViewModel createCustomer, ConvertSupplierViewModel convertSupplier)
    {
        CreateThirdParty = createCustomer;

        ConvertSupplier = convertSupplier;

        ConvertSupplier.PropertyChanged += (_, p) =>
        {
            if(p.PropertyName is nameof(ConvertSupplierViewModel.IsOpen) && !ConvertSupplier.IsOpen)
                SetPopup(Popups.None);
        };

        CreateThirdParty.PropertyChanged += (_,p) =>
        {
            if(p.PropertyName is nameof(CreateThirdPartyViewModel.IsOpen) && !CreateThirdParty.IsOpen)
                SetPopup(Popups.None);
        };
    
    }
    private enum Popups
    {
        NewCustomer = 1,
        FromSupplier = 2,
        None = 0
    }

    public CreateThirdPartyViewModel CreateThirdParty {get;}
    public ConvertSupplierViewModel ConvertSupplier {get;}
    public event Action? ViewIsClosed;
    public bool AnyPopupIsOpen => CurrentPopup != null;

    [ObservableProperty]
    public partial ThirdPartyDetailsDto? ThirdParty { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AnyPopupIsOpen))]
    public partial ViewModelBase? CurrentPopup {get; private set;}

    [ObservableProperty]
    public partial bool IsAddAddressOptionOpen {get; set;}
    
    [RelayCommand]
    private void OpenCustomerPopUp()
    => SetPopup(Popups.NewCustomer);

    [RelayCommand]
    private void CloseCustomerPopUp()
    => SetPopup(Popups.None);

    [RelayCommand]
    private void OpenConvertPopup()
    => SetPopup(Popups.FromSupplier);

    [RelayCommand]
    private void CloseConvertPopup()
    => SetPopup(Popups.None);

    [RelayCommand]
    private void ShowAddAddress()
    => IsAddAddressOptionOpen = true;

    [RelayCommand]
    private void CancelAddAddress()
    => IsAddAddressOptionOpen = false;

    private void SetPopup(Popups popup)
    {
        switch (popup)
        {
            case Popups.FromSupplier: 
                CreateThirdParty.IsOpen = false;
                ConvertSupplier.IsOpen = true;
                CurrentPopup = ConvertSupplier;
            break;   

            case Popups.NewCustomer:
                CreateThirdParty.IsOpen = true;
                ConvertSupplier.IsOpen = false;
                CurrentPopup = CreateThirdParty;
            break;

            default:
                CreateThirdParty.IsOpen = false;
                ConvertSupplier.IsOpen = false;
                CurrentPopup = null;
            break;
        };
    }
    
    [RelayCommand]
    public void Exit()
    {
        ThirdParty = null;

        ViewIsClosed?.Invoke();
    }
}