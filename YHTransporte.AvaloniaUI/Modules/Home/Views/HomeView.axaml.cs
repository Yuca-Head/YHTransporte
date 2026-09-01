using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace YHTransporte.AvaloniaUI.Modules.Home.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        ButtonsPanel.AddHandler(Button.ClickEvent, OnButtonClick);
        btnInicio.Background = _selectedColor;
    }

    private static readonly SolidColorBrush _selectedColor = new (Color.Parse("#0056D6"));

    public void MarkActive(string? buttonName)
    {
        foreach(Button b in ButtonsPanel.Children.OfType<Button>()) 
            if(b.Name == buttonName)    
                b.Background = _selectedColor;
            else
                b.Background = Brushes.Transparent;
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if(e.Source is not Button a || string.IsNullOrWhiteSpace(a.Name))
            return;
        
        MarkActive(a.Name);
    }
}