using CommunityToolkit.Mvvm.ComponentModel;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;

/// <summary>
/// Menú de conductores (misma lógica que ThirdPartyMenuViewModel): lista o editor.
/// </summary>
public partial class DriverMenuViewModel : ViewModelBase
{
    public DriverMenuViewModel(DriverListSectionViewModel list, DriverEditorViewModel editor)
    {
        _list = list;
        _editor = editor;

        _list.SelectedDriverChanged += () =>
        {
            if(_list.SelectedDriver is null)
                return;

            _editor.Driver = _list.SelectedDriver;
            CurrentView = _editor;
        };
        _editor.ViewIsClosed += () => CurrentView = _list;

        CurrentView = _list;
    }

    private readonly DriverListSectionViewModel _list;
    private readonly DriverEditorViewModel _editor;

    [ObservableProperty]
    public partial ViewModelBase CurrentView {get; private set;}
}
