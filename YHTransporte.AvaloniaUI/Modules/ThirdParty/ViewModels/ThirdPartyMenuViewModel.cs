using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.Views;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

public partial class ThirdPartyMenuViewModel : ViewModelBase
{
    public ThirdPartyMenuViewModel(ThirdPartyListSectionViewModel list, ThirdPartyEditorMenuViewModel editor)
    {
        _list = list;
        _editor = editor;
        _list.SelectedPartyChanged += () => 
        {
            _editor.ThirdParty = _list.SelectedThirdParty;
            CurrentView = _editor;  
        };
        _editor.ViewIsClosed += () => CurrentView = _list;

        CurrentView = _list;
    }

    private readonly ThirdPartyListSectionViewModel _list;
    private readonly ThirdPartyEditorMenuViewModel _editor; 

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}
}