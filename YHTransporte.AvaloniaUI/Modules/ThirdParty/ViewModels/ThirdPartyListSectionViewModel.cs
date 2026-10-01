using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public ThirdPartyListSectionViewModel(ThirdPartyContext context)
    {
        _context = context;

        ThirdParties = [.._context.ThirdParties];
        WeakReferenceMessenger.Default.Register<ThirdPartyUpdateMessage>(this, (_, _)=>
        ThirdParties = [.._context.ThirdParties]);  

    
    }

    public event Action? SelectedPartyChanged;
    private readonly ThirdPartyContext _context;

    [ObservableProperty]
    public partial ThirdPartyDetailsDto? SelectedThirdParty {get; set;}


    public ObservableCollection<ThirdPartyDetailsDto> ThirdParties {get; private set;}

    [RelayCommand]
    public void ChangeSelectedParty()
    {
        SelectedPartyChanged?.Invoke();
    }


}