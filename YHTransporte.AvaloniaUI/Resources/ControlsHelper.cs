using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace YHTransporte.AvaloniaUI.Resources;

public static class ControlsHelper
{
    public static void MarkSelectedButton(object? source, Controls controls, IBrush color)
    {
        if(source is not Button a || string.IsNullOrWhiteSpace(a.Name))
            return;

        foreach(Button b in controls.OfType<Button>()) 
            if(b.Name == a.Name)    
                b.Background = color;
            else
                b.Background = Brushes.Transparent;   
    }
}