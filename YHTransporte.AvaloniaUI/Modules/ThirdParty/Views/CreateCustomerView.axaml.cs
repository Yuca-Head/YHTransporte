
using Avalonia.Controls;
using Avalonia.Media;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.Views;

public partial class CreateCustomerView : UserControl
{

    public CreateCustomerView()
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
                    if(vm.HasError)
                        ResultTextBlock.Foreground = Brush.Parse("Red");
                    else
                        ResultTextBlock.Foreground = Brush.Parse("Green");
            };

            ResultTextBlock.Foreground = vm.HasError? Brush.Parse("Red") : Brush.Parse("Green");
        }

    }
}