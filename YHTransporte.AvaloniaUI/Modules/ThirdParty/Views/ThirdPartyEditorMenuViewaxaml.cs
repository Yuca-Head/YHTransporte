using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.Extra;
using YHTransporte.AvaloniaUI.Modules.ThirdParty.ViewModels;
using static YHTransporte.AvaloniaUI.Resources.ResultManagerExtensions;


namespace YHTransporte.AvaloniaUI.Modules.ThirdParty.Views;



public partial class ThirdPartyEditorMenuView : UserControl
{
    private readonly ResultManagerSubscription<ThirdPartyAddressCreator> _subscriber;

    public ThirdPartyEditorMenuView()
    {
        InitializeComponent();
        _subscriber = new(this, this.AddressResultMessage){Selector = () =>
        {
            if(this.DataContext is ThirdPartyEditorMenuViewModel vm)
                return vm.AddressCreator;
            else
                throw new InvalidOperationException();
        }
        };
        _subscriber.SubscribeToMrBeast();
    }

    



}