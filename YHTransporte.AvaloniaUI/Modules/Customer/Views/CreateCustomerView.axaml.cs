
using Avalonia.Controls;
using Avalonia.Media;
using YHTransporte.AvaloniaUI.Modules.Customer.ViewModels;

namespace YHTransporte.AvaloniaUI.Modules.Customer.Views;

public partial class CreateCustomerView : UserControl
{

    public CreateCustomerView()
    {

        InitializeComponent();

        this.DataContextChanged += (_,_) => AdaptContext();

    }

    private void AdaptContext()
    {
        if(DataContext is CreateCustomerViewModel vm)
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