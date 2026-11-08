using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Resources.Converters;

/// <summary>
/// Color de la "pastilla" de estado. Los colores salen de Resources/Themes/MainTheme.axaml
/// (PendingStatusBrush, InTransitStatusBrush, DoneStatusBrush, CanceledStatusBrush).
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string key = value switch
        {
            OrderStatuses.Pending or ShipmentStatuses.Pending => "PendingStatusBrush",
            OrderStatuses.InProgress or ShipmentStatuses.OnWay => "InTransitStatusBrush",
            OrderStatuses.Completed or ShipmentStatuses.Delivered => "DoneStatusBrush",
            OrderStatuses.Canceled or ShipmentStatuses.Canceled => "CanceledStatusBrush",

            true => "InTransitStatusBrush",   // En uso
            false => "DoneStatusBrush",       // Disponible

            _ => "PrimaryBrush"
        };

        if(global::Avalonia.Application.Current?.Resources.TryGetResource(key, null, out var resource) == true &&
        resource is IBrush brush)
            return brush;

        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    => throw new NotSupportedException();
}
