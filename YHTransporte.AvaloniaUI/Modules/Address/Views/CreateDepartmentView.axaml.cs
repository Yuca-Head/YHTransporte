using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Address.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;

namespace YHTransporte.AvaloniaUI.Modules.Address.Views;
public partial class CreateDepartmentView : UserControl
{
    private readonly ResultManagerSubscription<CreateDepartmentViewModel> _subscriber;
    public CreateDepartmentView()
    {
        InitializeComponent();
            
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }

}