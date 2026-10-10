using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Addresses.UseCases.CreateAddress;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Address.ViewModels;

public partial class CreateDepartmentViewModel(CreateAddressHandler createHandler) : ViewModelBase, IResultProvider
{
    private readonly CreateAddressHandler _handler = createHandler;

    [ObservableProperty]
    public partial AddressKey Department {get; set;} = new("", -1);

    [ObservableProperty]
    public partial bool IsOpen {get; set;}

    [ObservableProperty]
    public partial ResultManager ResultManager {get; private set;} = new(); 

    [RelayCommand]
    public async Task Create()
    {
    
        (await _handler.HandleDepartment([Department])).Switch
        (
            success => 
            {
                ResultManager.Success("Departamento Creado con éxito!!");
                WeakReferenceMessenger.Default.Send<DepartmentUpdateMessage>(new([], Shared.Enums.ContextChangeType.Creation));
            },
            validationError => {
                
                StringBuilder sb = new("Se han encontrado los siguientes errores:\n");
                
                validationError.Errors.ToList().ForEach(x => sb.Append($"{x}\n"));
                ResultManager.MarkError(sb.ToString());

            },
            alreadyExists => ResultManager.MarkError($"Ya existe un departamento {alreadyExists.Argument.FirstOrDefault()}"),
            repeatedValue => ResultManager.MarkError($"Esto no debería salir xdxdxdxdxdxdxdxdxdxddxdxdsx... Ayuda :c")
        );

    }   


    [RelayCommand]
    public void Close()
    => ResultManager.Clear(() => 
    {
        IsOpen = false;
        Department = new("", -1);
    });
    


}