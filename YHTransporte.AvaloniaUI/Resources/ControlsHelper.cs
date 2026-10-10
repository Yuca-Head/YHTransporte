using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using YHTransporte.AvaloniaUI.Shared;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.ViewModels;

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

    public static IBrush? ChangeColorByResult(bool hasError,
    IBrush? errorColor = null, IBrush? otherColor = null)
    => hasError? errorColor ?? Brush.Parse("Red") : otherColor ?? Brush.Parse("Green");



    



}