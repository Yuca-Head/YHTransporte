using System;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Modules.Address.ViewModels;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.Extra;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class ThirdPartyEditorMenuViewModel : ViewModelBase
{

    public ThirdPartyEditorMenuViewModel(CreateThirdPartyViewModel createCustomer, AddressContext addressContext, 
    ThirdPartyAddressCreator addressCreator, CreateMunicipalityViewModel createMunicipalityVM, 
    CreateDepartmentViewModel createDepartmentVM, ThirdPartyProductManager productManager)
    {
        CreateThirdParty = createCustomer;
        AddressContext = addressContext;
        AddressCreator = addressCreator;
        ProductManager = productManager;

        _createDepartmentVM = createDepartmentVM;
        _createMunicipalityVM = createMunicipalityVM;

        _createMunicipalityVM.PropertyChanged += (_,e) =>
        {
            if(e.PropertyName is nameof(_createMunicipalityVM.IsOpen))
                if(_createMunicipalityVM.IsOpen)
                {
                    _createDepartmentVM.IsOpen = false;
                    CurrentPopup = _createMunicipalityVM;
                }
                else if(!_createDepartmentVM.IsOpen)
                    CurrentPopup = null;
        };

        _createDepartmentVM.PropertyChanged += (_, e) =>
        {
            if(e.PropertyName is nameof(_createDepartmentVM.IsOpen))
                if(_createDepartmentVM.IsOpen)
                {
                    _createMunicipalityVM.IsOpen = false;
                    CurrentPopup = _createDepartmentVM;
                }
                else if(!_createMunicipalityVM.IsOpen)
                    CurrentPopup = null;
        };
    }


    public CreateThirdPartyViewModel CreateThirdParty {get;}

    //Supposed to be never changed, I hope so...
    [ObservableProperty]
    public partial ThirdPartyAddressCreator AddressCreator {get; private set;}

    // CAMBIO: productos del proveedor (solo se muestran si el tercero es proveedor).
    public ThirdPartyProductManager ProductManager {get;}

    [ObservableProperty]
    public partial AddressContext AddressContext {get; private set;}
    public event Action? ViewIsClosed;

    private readonly CreateMunicipalityViewModel _createMunicipalityVM;

    private readonly CreateDepartmentViewModel _createDepartmentVM;

    [ObservableProperty]
    public partial ThirdPartyDetailsDto? ThirdParty { get; set; }

    partial void OnThirdPartyChanged(ThirdPartyDetailsDto? oldValue, ThirdPartyDetailsDto? newValue)
    {
        if(newValue is null)
            return;

        AddressCreator.ThirdParty = newValue;
        ProductManager.ThirdParty = newValue;
    }

    public bool AnyPopupIsOpen => CurrentPopup != null;

    [ObservableProperty]    
    [NotifyPropertyChangedFor(nameof(AnyPopupIsOpen))]
    public partial ViewModelBase? CurrentPopup {get; private set;}

    [ObservableProperty]
    public partial bool IsAddAddressOptionOpen {get; set;}

    [RelayCommand]
    private void ShowAddAddress()
    => IsAddAddressOptionOpen = true;

    [RelayCommand]
    private void CancelAddAddress()
    {
        IsAddAddressOptionOpen = false;
        AddressCreator.Clear(); 
    } 

    [RelayCommand]
    private void OpenDepartmentCreator()
    => _createDepartmentVM.IsOpen = true;

    [RelayCommand]
    private void OpenMunicipalityCreator()
    => _createMunicipalityVM.IsOpen = true;


    [ObservableProperty]
    public partial bool IsAddProductOptionOpen {get; set;}

    [RelayCommand]
    private void ShowAddProduct()
    => IsAddProductOptionOpen = true;

    [RelayCommand]
    private void CancelAddProduct()
    => IsAddProductOptionOpen = false;

    [RelayCommand]
    private void Exit()
    {
        ThirdParty = null;
        CancelAddAddress();
        CancelAddProduct();
        ProductManager.Clear();
        ViewIsClosed?.Invoke();
    }
}