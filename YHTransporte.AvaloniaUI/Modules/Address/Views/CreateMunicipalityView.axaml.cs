using Avalonia.Controls;
using YHTransporte.AvaloniaUI.Modules.Address.ViewModels;
using YHTransporte.AvaloniaUI.Resources;

namespace YHTransporte.AvaloniaUI.Modules.Address.Views;

public partial class CreateMunicipalityView : UserControl
{

    private readonly ResultManagerExtensions.ResultManagerSubscription<CreateMunicipalityViewModel> _subscriber;
    
    public CreateMunicipalityView()
    {
        InitializeComponent();
        _subscriber = new(this, this.ResultTextBlock);
        _subscriber.SubscribeToMrBeast();
    }

}