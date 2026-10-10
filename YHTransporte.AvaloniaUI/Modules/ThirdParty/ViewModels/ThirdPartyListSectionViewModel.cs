using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Shared.Contexts;
using YHTransporte.AvaloniaUI.Shared.Messaging;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class ThirdPartyListSectionViewModel : ViewModelBase
{
    public ThirdPartyListSectionViewModel(ThirdPartyContext context, CreateThirdPartyViewModel vm)
    {
        _context = context;
        CreateThirdParty = vm;
        ThirdParties = [];
        UpdateList();
        _context.ThirdPartiesChanged += UpdateList;

    }

    public event Action? SelectedPartyChanged;
    
    private readonly ThirdPartyContext _context;

    [ObservableProperty]
    public partial ThirdPartyDetailsDto? SelectedThirdParty {get; set;}

    [ObservableProperty]
    public partial CreateThirdPartyViewModel CreateThirdParty {get; set;}
    [ObservableProperty]
    public partial ObservableCollection<ThirdPartyDetailsDto> ThirdParties {get; private set;}

    [RelayCommand]
    public void ChangeSelectedParty()
    {
        SelectedPartyChanged?.Invoke();
    }

    [RelayCommand]
    public void OpenCreator()
    => CreateThirdParty.IsOpen = true;

    private void UpdateList()
    {
        ThirdParties.Clear();

        foreach(var tp in _context.ThirdParties)
            ThirdParties.Add(tp);
    }
}