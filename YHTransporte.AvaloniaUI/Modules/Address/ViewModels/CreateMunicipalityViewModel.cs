using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OneOf.Types;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Address.ViewModels;

public partial class CreateMunicipalityViewModel : ViewModelBase, IResultProvider
{

    public CreateMunicipalityViewModel(CreateAddressHandler createHandler, AddressContext context)
    {
        _handler = createHandler;
        _context = context;

        _context.DepartmentsChanged += UpdateDepts;

        UpdateDepts();
    }
    private readonly CreateAddressHandler _handler;

    [ObservableProperty]
    public partial AddressKey Municipality {get; set;} = new("", -1);

    private readonly AddressContext _context;

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

    [ObservableProperty]
    public partial DepartmentDto? SelectedDepartment {get; set;}

    public ObservableCollection<DepartmentDto> Departments {get;} = [];

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new();

    partial void OnSelectedDepartmentChanged(DepartmentDto? oldValue, DepartmentDto? newValue)
    {
        if(newValue is not null && oldValue != newValue)
            Municipality = Municipality with {PlaceId = newValue.Id};
    }
    
    private void UpdateDepts()
    {
        SelectedDepartment = null;

        Departments.Clear();
        foreach(var dept in _context.Departments)
            Departments.Add(dept);
    }

    [RelayCommand]
    public async Task Create()
    {

        if(Municipality.PlaceId is <0)
        {
            ResultManager.MarkError("Debe escoger un departamento para registrar un nuevo municipio");

            return;
        }
    
        (await _handler.HandleMunicipality([Municipality])).Switch
        (
            success => 
            {
                ResultManager.Success("Municipio Creado con éxito!!");
                WeakReferenceMessenger.Default.Send<MuinicipalityUpdateMessage>(new([], Shared.Enums.ContextChangeType.Creation));
            },
            validationError => {
                
                StringBuilder sb = new("Se han encontrado los siguientes errores:\n");
                
                validationError.Errors.ToList().ForEach(x => sb.Append($"{x}\n"));
                ResultManager.MarkError(sb.ToString());

            },
            alreadyExists => ResultManager.MarkError($"Ya existe un municipio {alreadyExists.Argument.FirstOrDefault()}"),
            repeatedValue => ResultManager.MarkError($"Esto no debería salir xdxdxdxdxdxdxdxdxdxddxdxdsx... Ayuda :c"),
            departmentNotFound => ResultManager.MarkError($"No se encontró departamento con id: {Municipality.PlaceId}")
        );

    }

    [RelayCommand]
    public void Close()
    {
        ResultManager.Clear(() => 
        {
            IsOpen = false;
            SelectedDepartment = null;
            Municipality = new("", -1);
        });
    }
    

}