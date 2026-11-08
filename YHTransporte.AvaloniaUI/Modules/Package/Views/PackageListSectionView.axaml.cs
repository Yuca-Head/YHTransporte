using Avalonia.Controls;
using Avalonia.Input;
using YHTransporte.AvaloniaUI.Modules.Package.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Package.Views;

public partial class PackageListSectionView : UserControl
{
    public PackageListSectionView()
    {
        InitializeComponent();
    }

    private void Package_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if(this.DataContext is PackageListSectionViewModel vm)
            vm.SelectedPackageHasChanged();
    }
}
