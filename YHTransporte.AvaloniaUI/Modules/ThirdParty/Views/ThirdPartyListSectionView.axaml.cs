using Avalonia.Controls;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.Views;

public partial class ThirdPartyListSectionView : UserControl
{
    public ThirdPartyListSectionView()
    {
        InitializeComponent();
    }

    private void ThirdParty_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is ThirdPartyListSectionViewModel vm)
        {
            vm.ChangeSelectedParty();
        }
    }
}