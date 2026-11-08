using Avalonia.Controls;
using Avalonia.Input;
using YHTransporte.AvaloniaUI.Modules.Driver.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Driver.Views;

public partial class DriverListSectionView : UserControl
{
    public DriverListSectionView()
    {
        InitializeComponent();
    }

    private void Driver_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if(DataContext is DriverListSectionViewModel vm)
            vm.ChangeSelectedDriver();
    }
}
