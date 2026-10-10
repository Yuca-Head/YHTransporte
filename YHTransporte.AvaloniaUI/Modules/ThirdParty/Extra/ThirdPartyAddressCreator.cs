using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.Application.Addresses.UseCases.GetAddress;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.UseCases.CreateThirdPartyAddress;
using YHTransporte.AvaloniaUI.Modules.Address.ViewModels;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.Extra;

public partial class ThirdPartyAddressCreator : ObservableObject, IResultProvider
{

    public ThirdPartyAddressCreator(AddressContext context, CreateAddressHandler createHandler, GetAddressHandler getHandler,
    CreateThirdPartyAddressHandler tpAddressHandler)
    {
        _context = context;
        _createHandler = createHandler;
        _getHandler = getHandler;
        _createTpAddressHandler = tpAddressHandler;


        
        UpdateDepts();

        _context.DepartmentsChanged += UpdateDepts;
    }    
    private readonly AddressContext _context;
    private readonly CreateAddressHandler _createHandler;
    private readonly GetAddressHandler _getHandler;
    private readonly CreateThirdPartyAddressHandler _createTpAddressHandler;
    private bool addressCreatedOnSession;

    public ObservableCollection<DepartmentDto> Departments {get;} = [];

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();


    public ObservableCollection<AddressDetailsDto> ThirdPartyAddresses {get;} = [];

    public ObservableCollection<MunicipalityDto> Municipalities {get;} = [];

    [ObservableProperty]
    public partial DepartmentDto? SelectedDepartment {get; set;}
    [ObservableProperty]
    public partial MunicipalityDto? SelectedMunicipality {get; set;}
    [ObservableProperty]
    public partial ThirdPartyDetailsDto? ThirdParty {get; set;}
    [ObservableProperty]
    public partial string NewAddress {get; set;} = "";

    partial void OnThirdPartyChanged(ThirdPartyDetailsDto? oldValue, ThirdPartyDetailsDto? newValue)
    {
        if(oldValue is not null && addressCreatedOnSession)
            WeakReferenceMessenger.Default.Send<ThirdPartyUpdateMessage>(new([oldValue.Key], Shared.Enums.ContextChangeType.Modification));

        if(newValue is null)
            return;

        //For every "new" thirdparty
        addressCreatedOnSession = false;
        ThirdPartyAddresses.Clear();
        newValue.Addresses.ToList().ForEach(ThirdPartyAddresses.Add);
    }
    

    partial void OnSelectedDepartmentChanged(DepartmentDto? oldValue, DepartmentDto? newValue)
    {
        SelectedMunicipality = null;
        Municipalities.Clear();
        
        if(newValue is null)
            return;
        foreach(var m in _context.Municipalities.Where(mun => mun.Department.Id == newValue.Id))
            Municipalities.Add(m);

    }

    private void UpdateDepts()
    {
        Departments.Clear();
    
        foreach(var d in _context.Departments)
            Departments.Add(d); 
        SelectedDepartment = null; 
        SelectedMunicipality = null;
    }


    [RelayCommand]
    public async Task Create()
    {
        if(!Validate())
            return;

        var result = await _createHandler.HandleAddress([new(NewAddress, SelectedMunicipality!.Id)]);

        result.Switch(
            async success => await HandleCreationgResults(),
            validationError =>
            {
                StringBuilder sb = new("Se han encontrado los siguientes errores:\n");

                validationError.Errors.ToList().ForEach(x => sb.Append($"{x}\n"));
                ResultManager.MarkError(sb.ToString());
            },
            alreadyExists => ResultManager.MarkError($"Ya existe esa dirección registrada: {alreadyExists.Argument.First()}"),
            repeatedValue =>
            {
                StringBuilder sb = new("Se han repetido los siguientes valores (deben ser unicos): \n");

                repeatedValue.Argument.ToList().ForEach(x => sb.Append($"Valor: {x.Value} / Veces: {x.Times}"));

                ResultManager.MarkError(sb.ToString());
            },
            municipalityNotFound => ResultManager.MarkError($"No se encontró el municipio con Id: {municipalityNotFound.Ids.First()}")
        );

    }


    private bool Validate()
    {
        if(ThirdParty is null)
        {
            ResultManager.MarkError("Debe ingresar un tercero para registrar la dirección");
            return false;
        }
        if(SelectedDepartment is null)
        {
            ResultManager.MarkError("Debe elegir un departamento para registrar la dirección");
            return false;
        }
        if(SelectedMunicipality is null)
        {
            ResultManager.MarkError("Debe ingresar un municipio para registrar la dirección");
            return false;
        }
        if(string.IsNullOrWhiteSpace(NewAddress))
        {
            ResultManager.MarkError("Debe agregar una dirección");
            return false;
        }

        return true;

    }

    private async Task HandleCreationgResults()
    {
        AddressDetailsDto registeredAddress = (await _getHandler.GetAddressesByDescription([NewAddress])).First(x => x.Municipality.Id == SelectedMunicipality!.Id);
        var result = await _createTpAddressHandler.Handle(new (registeredAddress.Id, ThirdParty!.Key));


        result.Switch
        (
            success => {
                ResultManager.Success("Dirección creada correctamente");
                addressCreatedOnSession = true;
                ThirdPartyAddresses.Add(registeredAddress);
            },
            thirdPartyNotFound => ResultManager.MarkError($"Error inesperado ha ocurrido; Tipo: {nameof(thirdPartyNotFound)} / "+
                thirdPartyNotFound.Key), 
            addressNotFound => 
            ResultManager.MarkError(
            $"Error inesperado. Tipo: {nameof(addressNotFound)} / " +
            $"{string.Join(" - ", addressNotFound.Ids)}"),
            //De hecho, esto no debería ocurrir, ya que en teoría no deberían haber direcciones duplicadas.
            alreadyContainsItem => ResultManager.MarkError("Este tercero ya contiene esta dirección")
        );
        


    }

    public void Clear()
    => ResultManager.Clear(() =>
    {
        ThirdParty = null;
        SelectedDepartment = null;
        SelectedMunicipality = null;
    });
    


}