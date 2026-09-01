using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<CloseApplicationMessage>
        (this, (_,_) => CloseApp());
    }

    private void CloseApp()
    => this.Close();
}