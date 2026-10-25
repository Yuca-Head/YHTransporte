
using Avalonia.Controls;
using Avalonia.Media;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;
using YHTransporte.AvaloniaUI.Resources;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.Views;

public partial class CreateThirdPartyView : UserControl
{

    public CreateThirdPartyView()
    {

        InitializeComponent();

        this.DataContextChanged += (_,_) => AdaptContext();

    }

    private void AdaptContext()
    {
        if(DataContext is CreateThirdPartyViewModel vm)
        {
            vm.PropertyChanged += (_, e) =>
            {
                if(e.PropertyName is nameof(vm.HasError))
                    ResultTextBlock.Foreground = ControlsHelper.ChangeColorByResult(vm.HasError);
            };

            ResultTextBlock.Foreground = vm.HasError? Brush.Parse("Red") : Brush.Parse("Green");
        }

    }
}