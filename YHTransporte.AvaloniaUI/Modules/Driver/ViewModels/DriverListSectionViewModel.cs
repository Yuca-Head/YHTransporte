using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YHTransporte.Application.Shipments.Dtos;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;

public partial class DriverListSectionViewModel : ViewModelBase
{
    public DriverListSectionViewModel(DriverContext context, CreateDriverViewModel creator)
    {
        _context = context;
        CreateDriver = creator;

        UpdateList();
        _context.DriversChanged += UpdateList;
    }

    private readonly DriverContext _context;

    public event Action? SelectedDriverChanged;

    [ObservableProperty]
    public partial DriverDto? SelectedDriver {get; set;}

    [ObservableProperty]
    public partial CreateDriverViewModel CreateDriver {get; set;}

    [ObservableProperty]
    public partial string SearchText {get; set;} = "";

    public ObservableCollection<DriverDto> Drivers {get;} = [];

    [RelayCommand]
    public void ChangeSelectedDriver()
    => SelectedDriverChanged?.Invoke();

    [RelayCommand]
    public void OpenCreator()
    => CreateDriver.IsOpen = true;

    partial void OnSearchTextChanged(string value)
    => UpdateList();

    private void UpdateList()
    {
        string search = SearchText.Trim();

        Drivers.Clear();

        foreach(var driver in _context.Drivers.Where(x => search.Length == 0 ||
        x.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        x.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)))
            Drivers.Add(driver);
    }
}
