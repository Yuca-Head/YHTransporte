using System;
using Avalonia.Controls;
using Avalonia.Media;
using YHTransporte.AvaloniaUI.Shared.Abstractions;
using YHTransporte.AvaloniaUI.ViewModels;

namespace YHTransporte.AvaloniaUI.Resources;


public static class ResultManagerExtensions
{


    public sealed record class ResultManagerSubscription<T> : IDisposable
    where T : IResultProvider
    {
        private UserControl UserControl {get;}
        private object Control {get;}
        public Func<T>? Selector {get; init;} = null;
        public ResultManagerSubscription(UserControl userControl, object control)
        {
            UserControl = userControl;
            Control = control;
        }

        public void Dispose()
        {
            UserControl.DataContextChanged -= UpdateDataContextColors;
        }

        public void SubscribeToMrBeast()
        {
            UserControl.DataContextChanged += UpdateDataContextColors;
        }

        private void UpdateDataContextColors(object? sender, EventArgs e)
        {
            T dc;

            if(Selector is not null)
                dc = Selector();
            else if(UserControl.DataContext is T vm)
                dc = vm;
            else
                return;

            var foreground = Control.GetType().GetProperty("Foreground");

            if (foreground is not null && foreground.CanWrite)
            {   
                dc.ResultManager.GotError += () => foreground.SetValue(Control, SolidColorBrush.Parse("Red"));
                dc.ResultManager.GotSuccess += () => foreground.SetValue(Control, SolidColorBrush.Parse("Green"));

                Dispose();
            }   
            
        }


    }
}